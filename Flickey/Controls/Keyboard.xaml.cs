using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Newtonsoft.Json;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;

namespace Flickey.Controls
{
    using KeyboardComponents;

    /// <summary>
    /// Interaction logic for Keyboard.xaml
    /// </summary>
    public partial class Keyboard : UserControl, IDisposable
    {
        //  TouchMoveイベントが消失してから、入力操作の終了判定までのタイムアウト。
        private readonly TimeSpan moveEventTimeout = TimeSpan.FromSeconds(0.3);

        //  入力操作のホールド操作だと判定されるまでのタイムアウト。
        private readonly TimeSpan holdDetectionTimeout = TimeSpan.FromSeconds(0.5);

        private readonly CompositeDisposable disposable = new CompositeDisposable();

        //  5x5=25個のキーを保持しておく。
        private readonly Key[][] keys = new Key[5][];

        //  25個のキーすべてのTouch系のイベントをまとめて通知する。
        private readonly IObservable<(Key key, TouchEventArgs args)> touchDownStream;
        private readonly IObservable<(Key key, TouchEventArgs args)> touchUpStream;
        private readonly IObservable<(Key key, TouchEventArgs args)> touchMoveStream;

        //  入力操作中かどうかを通知・保持する。
        private readonly ReadOnlyReactiveProperty<bool> isInputOperationInProgress;

        //  入力操作の対象のキーとデバイスIDを通知・保持する。
        private readonly ReadOnlyReactiveProperty<(Key key, int? deviceId)> inputOperationTarget;

        //  入力操作の種類を通知する。
        private readonly ReadOnlyReactiveProperty<OperationType> operationType;

        //  入力操作時の相対的な指の位置を通知する。
        //  具体的な座標ではなく、上下左右などで表される。
        private readonly ReadOnlyReactiveProperty<FingerPos> fingerPos;

        /// <summary>
        /// KeyboardTypeプロパティの依存関係プロパティ。
        /// </summary>
        public static readonly DependencyProperty KeyboardTypeProperty =
            DependencyProperty.Register(nameof(KeyboardType), typeof(KeyboardType), typeof(Keyboard), new PropertyMetadata(KeyboardType.English, OnKeyboardTypeChanged));

        /// <summary>
        /// 入力確定時に呼ばれるコマンドの依存関係プロパティ。
        /// </summary>
        public static readonly DependencyProperty CommandProperty =
            DependencyProperty.Register(nameof(Command), typeof(ICommand), typeof(Keyboard));

        /// <summary>
        /// キーボードの種類を取得・設定します。
        /// 依存関係プロパティです。
        /// </summary>
        public KeyboardType KeyboardType
        {
            get => (KeyboardType)this.GetValue(KeyboardTypeProperty);
            set => this.SetValue(KeyboardTypeProperty, value);
        }

        /// <summary>
        /// 入力確定時に呼ばれるコマンド。
        /// </summary>
        public ICommand Command
        {
            get => (ICommand)this.GetValue(CommandProperty);
            set => this.SetValue(CommandProperty, value);
        }

        /// <summary>
        /// インスタンスを初期化します。
        /// </summary>
        public Keyboard()
        {
            InitializeComponent();

            //  25個のキーを配列に格納する。
            for (int y = 0; y < 5; y++)
            {
                this.keys[y] = new Key[5];
                for (int x = 0; x < 5; x++)
                {
                    var name = $"Key{y}{x}";
                    var key = (Key)this.FindName(name);
                    this.keys[y][x] = key.AddTo(this.disposable);
                }
            }

            //  すべてのイベントをまとめておく。
            //  それらをHot変換しておく。
            this.touchDownStream = this.keys
                .SelectMany(k => k)
                .Select(key => key.TouchDownAsObservable.Select(args => (key, args)))
                .Merge()
                .Where(_ => !this.isInputOperationInProgress.Value)
                .Publish();

            this.touchUpStream = this.keys
                .SelectMany(k => k)
                .Select(key => key.TouchUpAsObservable.Select(args => (key, args)))
                .Merge()
                .Where(tuple => tuple.args.TouchDevice.Id == this.inputOperationTarget.Value.deviceId)
                .Publish();

            this.touchMoveStream = this.keys
                .SelectMany(k => k)
                .Select(key => key.TouchMoveAsObservable.Select(args => (key, args)))
                .Merge()
                .Where(tuple => tuple.args.TouchDevice.Id == this.inputOperationTarget.Value.deviceId)
                .Publish();

            //  手動でConnectを呼んでおく。
            //  破棄はDispose時にまとめてやる。
            (this.touchDownStream as IConnectableObservable<(Key, TouchEventArgs)>).Connect().AddTo(this.disposable);
            (this.touchUpStream as IConnectableObservable<(Key, TouchEventArgs)>).Connect().AddTo(this.disposable);
            (this.touchMoveStream as IConnectableObservable<(Key, TouchEventArgs)>).Connect().AddTo(this.disposable);

            //  入力操作の対象 (キーとデバイスID) を通知する。
            var targetStream = Observable.Merge(
                this.touchDownStream.Select(tuple => (tuple.key, (int?)tuple.args.TouchDevice.Id)),
                this.touchUpStream.Select(_ => ((Key)null, (int?)null)),
                this.touchMoveStream.Throttle(this.moveEventTimeout).ObserveOnDispatcher().Select(_ => ((Key)null, (int?)null)))
                .Publish();
            targetStream.Connect().AddTo(this.disposable);

            //  キーとデバイスIDを通知するプロパティと、
            //  それらがnullでないかどうか、つまり入力操作中かどうかを通知するプロパティを作る。
            this.inputOperationTarget = targetStream.ToReadOnlyReactiveProperty().AddTo(this.disposable);
            this.isInputOperationInProgress = targetStream.Select(target => target.Item1 != null && target.Item2 != null).ToReadOnlyReactiveProperty().AddTo(this.disposable);

            //  指の位置を通知する。
            var fingerPosStream = Observable.WithLatestFrom(
                this.touchMoveStream,
                this.touchDownStream,
                (current, reference) =>
                {
                    //  基準のキーの中心からの位置をベクトルで取得する。
                    var width = reference.key.ActualWidth;
                    var height = reference.key.ActualHeight;
                    var relativePos = current.args.GetTouchPoint(reference.key).Position;
                    var vec = new Vector(relativePos.X - width / 2, -(relativePos.Y - height / 2));

                    //  X成分とY成分の絶対値がそれぞれ、幅と高さの半分よりも小さければ、基準のキーからはみ出していない。
                    if (Math.Abs(vec.X) <= width / 2 && Math.Abs(vec.Y) <= height / 2) return FingerPos.Neutral;

                    //  変位ベクトルと基準のキーの右上方向ベクトルとのなす角が正ならば、左または上になる。
                    else if (Vector.AngleBetween(new Vector(width, height), vec) > 0)
                    {
                        //  変位ベクトルと基準のキーの左上方向ベクトルとのなす角が、
                        //  正ならば左になり、負ならば上となる。
                        return (Vector.AngleBetween(new Vector(-width, height), vec) > 0) ? FingerPos.Left : FingerPos.Top;
                    }

                    //  なす角が負ならば、右または下になる。
                    else
                    {
                        //  変位ベクトルと基準のキーの右下方向ベクトルとのなす角が、
                        //  正ならば右になり、負ならば下となる。
                        return (Vector.AngleBetween(new Vector(width, -height), vec) > 0) ? FingerPos.Right : FingerPos.Bottom;
                    }
                }).Publish();
            fingerPosStream.Connect().AddTo(this.disposable);

            this.fingerPos = fingerPosStream.ToReadOnlyReactiveProperty().AddTo(this.disposable);

            //  スライド操作の判定をする。
            var slideOperationDetectionStream = fingerPosStream
                .Where(pos => pos != FingerPos.Neutral)
                .Select(_ => OperationType.Slide);

            //  ホールド操作の判定をする。
            //  入力処理が始まって、一定時間後にホールド操作であると判定する。
            //  一定時間待機中にデバイスIDが変わった場合は無効にする。
            var holdOperationDetectionStream = this.inputOperationTarget
                .Select(target => target.deviceId)
                .Where(id => id != null)
                .Delay(this.holdDetectionTimeout)
                .ObserveOnDispatcher()
                .Where(id => id == this.inputOperationTarget.Value.deviceId)
                .Select(_ => OperationType.Hold);

            //  入力操作の種類を通知する。
            this.operationType = Observable.Merge(
                this.isInputOperationInProgress.Select(value => value ? OperationType.Tap : OperationType.None),
                slideOperationDetectionStream,
                holdOperationDetectionStream)
                .Pairwise()
                .Where(pair =>
                    !((pair.NewItem == OperationType.Slide && pair.OldItem != OperationType.Tap)
                        || (pair.NewItem == OperationType.Hold && pair.OldItem != OperationType.Tap)))
                .Select(pair => pair.NewItem)
                .ToReadOnlyReactiveProperty()
                .AddTo(this.disposable);

            //  キーの印字を設定する。
            this.SetKeyLabels();

            //  キーにストリームを渡して、状態をリフレッシュさせる。
            for (int y = 0; y < 5; y++)
            {
                for (int x = 0; x < 5; x++)
                {
                    var key = this.keys[y][x];
                    key.Refresh();
                    key.SetOperationStreams(
                        this.operationType.Select(type => (this.inputOperationTarget.Value.key, type)).ToReadOnlyReactiveProperty().AddTo(this.disposable),
                        this.fingerPos,
                        this.OnCharacterReceived);
                }
            }
        }

        /// <summary>
        /// リソースの破棄を行います。
        /// </summary>
        public void Dispose()
        {
            this.disposable.Dispose();
        }
        
        //  キーボードの種類が変更されたとき。
        private static void OnKeyboardTypeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var keyboard = (Keyboard)d;
            var type = (KeyboardType)e.NewValue;
            
            for (int y = 0; y < 5; y++)
            {
                for (int x = 0; x < 5; x++)
                {
                    var key = keyboard.keys[y][x];
                    key.KeyboardType = type;
                }
            }
        }

        //  文字の入力が確定されたとき。
        private void OnCharacterReceived(string character)
        {
            //  キーボードの切り替えをする。
            switch (character)
            {
                case "Ctrl+α": this.KeyboardType = KeyboardType.Shortcuts; return;
                case "☆123": this.KeyboardType = KeyboardType.Number; return;
                case "ABC": this.KeyboardType = KeyboardType.English; return;
                case "あいう": this.KeyboardType = KeyboardType.Japanese; return;
            }

            //  一時的な処理だけども、アルファベットを小文字にする。
            if (character.Length == 1)
            {
                var c = character[0];
                if ('A' <= c && c <= 'Z')
                    character = char.ToLower(c).ToString();
            }

            Command.Execute(character);
        }

        //  各キーにラベルを割り当てる。
        private void SetKeyLabels()
        {
            var fileNames = new[] { "Shortcuts.json", "Number.json", "English.json", "Japanese.json" };

            //  エラー時の処理は検討中だが、ダミーデータとして「*」が表示されるデータを流す。
            var dummy = Enumerable.Range(1, 25)
                .Select(_ => new KeyLabel { LabelStyle = LabelDisplayStyle.OnlyFirstCharacter, Characters = new[] { "*", null, null, null, null } })
                .ToList();

            //  JSONから印字データを読み取り、各キーに設定していく。
            fileNames.ToObservable()
                .Select(name => JsonConvert.DeserializeObject<List<KeyLabel>>(File.ReadAllText(name)))
                .OnErrorResumeNext(Observable.Return(dummy))
                .ToArray()
                .Subscribe(labels =>
                {
                    for (var y = 0; y < 5; y++)
                    {
                        for (var x = 0; x < 5; x++)
                        {
                            var index = y * 5 + x;
                            var key = this.keys[y][x];

                            key.Labels = Enumerable.Range(0, 4).Select(n => labels[n][index]).ToArray();
                        }
                    }
                });
        }

    }
}