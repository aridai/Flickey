﻿using System;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
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

        //  入力操作時のスライド入力とみなされる指の移動量のしきい値。
        private readonly float distanceThreshold = 64.0f;

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
        private readonly ReadOnlyReactiveProperty<InputOperationType> operationType;

        //  入力操作時の相対的な指の位置を通知する。
        //  具体的な座標ではなく、上下左右などで表される。
        private readonly ReadOnlyReactiveProperty<RelativeFingerPos> fingerPos;

        /// <summary>
        /// 入力確定時に呼ばれるコマンドの依存関係プロパティ。
        /// </summary>
        public static readonly DependencyProperty CommandProperty =
            DependencyProperty.Register(nameof(Command), typeof(ICommand), typeof(Keyboard));

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
            //  脳死で.AddTo(this.Disposable)をしているけど、ところどころ不要なものもある気がする。
            //  そこはご愛嬌だが、気が向いたら考え直す。

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
                this.touchMoveStream.Select(tuple => tuple.args.GetTouchPoint(null).Position),
                this.touchDownStream.Select(tuple => tuple.args.GetTouchPoint(null).Position),
                (current, starting) => current - starting)
                .Select(vec =>
                {
                    if (Math.Abs(vec.X) > this.distanceThreshold || Math.Abs(vec.Y) > this.distanceThreshold)
                    {
                        if (Math.Abs(vec.X) > Math.Abs(vec.Y)) return (vec.X > 0) ? RelativeFingerPos.Right : RelativeFingerPos.Left;
                        else return (vec.Y > 0) ? RelativeFingerPos.Bottom : RelativeFingerPos.Top;
                    }

                    return RelativeFingerPos.Neutral;
                }).Publish();
            fingerPosStream.Connect().AddTo(this.disposable);

            this.fingerPos = fingerPosStream.ToReadOnlyReactiveProperty().AddTo(this.disposable);

            //  スライド操作の判定をする。
            var slideOperationDetectionStream = fingerPosStream
                .Where(pos => pos != RelativeFingerPos.Neutral)
                .Select(_ => InputOperationType.Slide);

            //  ホールド操作の判定をする。
            //  入力処理が始まって、一定時間後にホールド操作であると判定する。
            //  一定時間待機中にデバイスIDが変わった場合は無効にする。
            var holdOperationDetectionStream = this.inputOperationTarget
                .Select(target => target.deviceId)
                .Where(id => id != null)
                .Delay(this.holdDetectionTimeout)
                .ObserveOnDispatcher()
                .Where(id => id == this.inputOperationTarget.Value.deviceId)
                .Select(_ => InputOperationType.Hold);

            //  入力操作の種類を通知する。
            this.operationType = Observable.Merge(
                this.isInputOperationInProgress.Select(value => value ? InputOperationType.Tap : InputOperationType.None),
                slideOperationDetectionStream,
                holdOperationDetectionStream)
                .Pairwise()
                .Where(pair =>
                    !((pair.NewItem == InputOperationType.Slide && pair.OldItem != InputOperationType.Tap)
                        || (pair.NewItem == InputOperationType.Hold && pair.OldItem != InputOperationType.Tap)))
                .Select(pair => pair.NewItem)
                .ToReadOnlyReactiveProperty()
                .AddTo(this.disposable);

            //this.inputOperationTarget.Subscribe(tuple => System.Diagnostics.Debug.WriteLine((tuple.key != null && tuple.deviceId != null) ? $"入力中 キー:({tuple.key?.Row},{tuple.key?.Column}), デバイスID:{tuple.deviceId}" : "非入力中"));
            //this.operationType.Subscribe(type => System.Diagnostics.Debug.WriteLine($"操作タイプ:{type}"));
            //this.fingerPos.Subscribe(pos => System.Diagnostics.Debug.WriteLine($"相対位置:{pos}"));
        }

        /// <summary>
        /// リソースの破棄を行います。
        /// </summary>
        public void Dispose()
        {
            this.disposable.Dispose();
        }
    }
}