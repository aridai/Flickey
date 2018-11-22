using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;

namespace Flickey.Controls
{
    using KeyboardComponents;

    /// <summary>
    /// Interaction logic for Key.xaml
    /// </summary>
    public partial class Key : UserControl, IDisposable
    {
        private readonly CompositeDisposable disposable = new CompositeDisposable();

        private int? raw;

        private int? column;

        private KeyboardType keyboardType;

        /// <summary>
        /// TouchDownイベントをIObservableへ変換したストリームを取得します。
        /// </summary>
        public IObservable<TouchEventArgs> TouchDownAsObservable { get; }

        /// <summary>
        /// TouchUpイベントをIObservableへ変換したストリームを取得します。
        /// </summary>
        public IObservable<TouchEventArgs> TouchUpAsObservable { get; }

        /// <summary>
        /// TouchMoveイベントをIObservableへ変換したストリームを取得します。
        /// </summary>
        public IObservable<TouchEventArgs> TouchMoveAsObservable { get; }

        /// <summary>
        /// GridのRowの値を取得します。
        /// </summary>
        public int Row => this.raw ?? (int)(this.raw = Grid.GetRow(this));

        /// <summary>
        /// GridのColumnの値を取得します。
        /// </summary>
        public int Column => this.column ?? (int)(this.column = Grid.GetColumn(this));

        /// <summary>
        /// キーボードの種類を取得・設定します。
        /// </summary>
        public KeyboardType KeyboardType
        {
            get => this.keyboardType;
            set
            {
                this.keyboardType = value;
                this.Refresh();
            }
        }

        /// <summary>
        /// キーボードの文字の集合のコレクションを取得・設定します。
        /// </summary>
        public IReadOnlyList<CharacterSet> CharacterSets { get; set; }

        /// <summary>
        /// 現在選択中のキーボードの文字の集合を取得します。
        /// </summary>
        public CharacterSet CurrentCharacterSet => this.CharacterSets[(int)this.KeyboardType];

        /// <summary>
        /// Shapeプロパティの依存関係プロパティ。
        /// </summary>
        public static DependencyProperty ShapeProperty
            = DependencyProperty.Register(nameof(Shape), typeof(KeyShape), typeof(Key), new PropertyMetadata(KeyShape.Normal));

        /// <summary>
        /// KeyEffectプロパティの依存関係プロパティ。
        /// </summary>
        public static DependencyProperty KeyEffectProperty
            = DependencyProperty.Register(nameof(KeyEffect), typeof(KeyEffect), typeof(Key), new PropertyMetadata(KeyEffect.NoEffect));

        /// <summary>
        /// LabelStyleプロパティの依存関係プロパティ。
        /// </summary>
        public static DependencyProperty LabelStyleProperty
            = DependencyProperty.Register(nameof(LabelStyle), typeof(LabelStyle), typeof(Key), new PropertyMetadata(LabelStyle.OnlyFirstCharacter));

        /// <summary>
        /// PrimaryTextプロパティの依存関係プロパティ。
        /// </summary>
        public static DependencyProperty PrimaryTextProperty
            = DependencyProperty.Register(nameof(PrimaryText), typeof(string), typeof(Key), new PropertyMetadata(string.Empty));

        /// <summary>
        /// SecondaryTextプロパティの依存関係プロパティ。
        /// </summary>
        public static DependencyProperty SecondaryTextProperty
            = DependencyProperty.Register(nameof(SecondaryText), typeof(string), typeof(Key), new PropertyMetadata(default(string)));

        /// <summary>
        /// キーの形を取得・設定します。
        /// 依存関係プロパティです。
        /// </summary>
        public KeyShape Shape
        {
            get => (KeyShape)this.GetValue(ShapeProperty);
            set => this.SetValue(ShapeProperty, value);
        }

        /// <summary>
        /// キーのエフェクトを取得・設定します。
        /// 依存関係プロパティです。
        /// </summary>
        public KeyEffect KeyEffect
        {
            get => (KeyEffect)this.GetValue(KeyEffectProperty);
            set => this.SetValue(KeyEffectProperty, value);
        }

        /// <summary>
        /// キーの印字の表示方法を取得・設定します。
        /// 依存関係プロパティです。
        /// </summary>
        public LabelStyle LabelStyle
        {
            get => (LabelStyle)this.GetValue(LabelStyleProperty);
            set => this.SetValue(LabelStyleProperty, value);
        }

        /// <summary>
        /// キーの印字を取得・設定します。
        /// 依存関係プロパティです。
        /// </summary>
        public string PrimaryText
        {
            get => (string)this.GetValue(PrimaryTextProperty);
            set => this.SetValue(PrimaryTextProperty, value);
        }

        /// <summary>
        /// キーの印字の2行目を取得・設定します。
        /// 依存関係プロパティです。
        /// </summary>
        public string SecondaryText
        {
            get => (string)this.GetValue(SecondaryTextProperty);
            set => this.SetValue(SecondaryTextProperty, value);
        }

        /// <summary>
        /// インスタンスを初期化します。
        /// </summary>
        public Key()
        {
            InitializeComponent();

            //  イベントをIO<T>化する。
            //  Keyboard側でHot変換するため、ここではColdで返す。
            this.TouchDownAsObservable = Observable.FromEvent<EventHandler<TouchEventArgs>, TouchEventArgs>(
                onNext => (sender, args) => onNext(args),
                handler => this.TouchDown += handler,
                handler => this.TouchDown -= handler);

            this.TouchUpAsObservable = Observable.FromEvent<EventHandler<TouchEventArgs>, TouchEventArgs>(
                onNext => (sender, args) => onNext(args),
                handler => this.TouchUp += handler,
                handler => this.TouchUp -= handler);

            this.TouchMoveAsObservable = Observable.FromEvent<EventHandler<TouchEventArgs>, TouchEventArgs>(
                onNext => (sender, args) => onNext(args),
                handler => this.PreviewTouchMove += handler,
                handler => this.PreviewTouchMove -= handler);
        }

        /// <summary>
        /// 入力操作を通知するストリームを設定します。
        /// </summary>
        /// <param name="input">入力操作の対象と種類を通知・保持するストリーム。</param>
        /// <param name="pos">指の相対位置を通知・保持するストリーム。</param>
        /// <param name="sender">入力が確定した文字を送信するコールバック。</param>
        public void SetOperationStreams(ReadOnlyReactiveProperty<(Key key, OperationType type)> type, ReadOnlyReactiveProperty<FingerPos> pos, Action<string> sender)
        {
            //  自分がタップされたとき。
            type.Where(tuple => tuple.type == OperationType.Tap)
                .Where(tuple => tuple.key == this)
                .Subscribe(_ => this.OnTapped())
                .AddTo(this.disposable);

            //  入力操作が終わったとき。
            type.Pairwise()
                .Where(list => list.NewItem.type == OperationType.None)
                .Subscribe(list => this.OnInputLeft(list.OldItem.key, pos.Value, sender))
                .AddTo(this.disposable);

            //  ホールド操作に入ったとき。
            type.Where(tuple => tuple.type == OperationType.Hold)
                .Subscribe(tuple => this.OnHolded(tuple.key))
                .AddTo(this.disposable);

            //  スライド・ホールド操作中に指が動いたとき。
            Observable.CombineLatest(type, pos, (t, p) => (t.key, t.type, p))
                .Where(tuple => tuple.type == OperationType.Slide || tuple.type == OperationType.Hold)
                .Subscribe(tuple => this.OnFingerPosChanged(tuple.key, tuple.type, tuple.p))
                .AddTo(this.disposable);
        }

        /// <summary>
        /// キーの印字やエフェクトなどの状態をリフレッシュします。
        /// </summary>
        public void Refresh()
        {
            //  形をリセットする。
            this.Shape = KeyShape.Normal;
            this.KeyEffect = KeyEffect.NoEffect;
            this.LabelStyle = this.CurrentCharacterSet.LabelStyle;

            //  印字の表示方法に応じて印字を設定する。
            var chars = this.CurrentCharacterSet.Characters.Where(c => c != null).ToArray();
            this.PrimaryText = (this.LabelStyle == LabelStyle.OneLine) ? chars.Aggregate(string.Empty, (total, next) => total + next) : chars.First();
            this.SecondaryText = chars.Skip(1).Aggregate(string.Empty, (total, next) => total + next);
        }

        /// <summary>
        /// リソースの破棄を行います。
        /// </summary>
        public void Dispose()
        {
            this.disposable.Dispose();
        }

        //  キーボードの種類が変わったとき。
        private static void OnKeyboardTypeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var key = (Key)d;
        }

        //  タップされたとき。
        private void OnTapped()
        {
            this.KeyEffect = KeyEffect.Focused;
        }

        //  指が離されたとき。
        private void OnInputLeft(Key target, FingerPos pos, Action<string> sender)
        {
            //  印字や形などをリセットする。
            this.Refresh();

            //  入力が確定した文字を送信する。
            var grid = this.GetAdjacentGridNums(target, pos);
            if (grid == (this.Row, this.Column))
            {
                var character = target.CurrentCharacterSet.Characters[(int)pos];
                sender(character);
            }
        }

        //  ホールド操作に入ったとき。
        private void OnHolded(Key target)
        {
            //  とりあえず、ブラー効果をつける。
            //  ホールド操作のポップ表示の対象になる場合はブラー効果は外れる。
            this.KeyEffect = KeyEffect.Blurred;

            //  操作対象のキーの持つ文字の配列を取得しておく。
            //  この配列は中央・左・上・右・下の順に文字が格納されていて、
            //  nullのときは、ホールド時のキーを表示させない。
            var chars = target.CurrentCharacterSet.Characters.TakeWhile(character => character != null).ToArray();

            //  文字がポップアップ表示されるところにあるキーの形を変更しておく。
            for (int i = 0; i < chars.Length; i++)
            {
                var grid = this.GetAdjacentGridNums(target, FingerPos.Neutral + i);
                if (grid == (this.Row, this.Column))
                {
                    this.Shape = KeyShape.HoldCenter + i;
                    this.KeyEffect = (i == 0) ? KeyEffect.Focused : KeyEffect.NoEffect;
                    this.PrimaryText = chars[i];
                    return;
                }
            }
        }

        //  指が動いたとき。
        private void OnFingerPosChanged(Key target, OperationType type, FingerPos pos)
        {
            //  ホールド操作のとき。
            if (type == OperationType.Hold)
            {
                var num = target.CurrentCharacterSet.Characters.TakeWhile(character => character != null).Count();
                if ((int)pos < num)
                {
                    //  指の位置にあるキーの形を変更する。
                    var grid = this.GetAdjacentGridNums(target, pos);
                    if (grid == (this.Row, this.Column))
                        this.KeyEffect = KeyEffect.Focused;
                }
            }

            //  スライド操作のとき。
            else
            {
                //  操作対象のキーの持つ文字の配列を取得しておく。
                var chars = target.CurrentCharacterSet.Characters
                    .TakeWhile(character => character != null).ToArray();

                //  スライド可能な向きにあるキーをループで回す。
                for (int i = 0; i < chars.Length; i++)
                {
                    var currentPos = FingerPos.Neutral + i;
                    var grid = this.GetAdjacentGridNums(target, FingerPos.Neutral + i);

                    if (grid == (this.Row, this.Column))
                    {
                        //  指がある位置にあるキーならば。
                        if (currentPos == pos)
                        {
                            this.Shape = KeyShape.Normal + i;
                            this.PrimaryText = chars[i];
                        }

                        //  指がない位置にあるキーならば。
                        else this.Refresh();
                    }
                }

                //  中心の印字を空にする。
                if (pos != FingerPos.Neutral)
                {
                    target.Shape = KeyShape.Empty;
                }
            }
        }

        //  隣接したキーのGridの番号を返す。
        //  そのキーが存在しない場合は(-1, -1)を返す。
        private (int, int) GetAdjacentGridNums(Key key, FingerPos pos)
        {
            var row = key.Row;
            var colmn = key.Column;
            (int row, int column) grid;

            //  GridのRowとColumnを計算する。
            switch (pos)
            {
                case FingerPos.Neutral: grid = (row, colmn); break;
                case FingerPos.Top: grid = (row - 1, colmn); break;
                case FingerPos.Bottom: grid = (row + 1, colmn); break;
                case FingerPos.Right: grid = (row, colmn + 1); break;
                case FingerPos.Left: grid = (row, colmn - 1); break;
                default: return (-1, -1);
            }

            //  番号が0から4までに収まっていない場合は(-1,-1)とする。
            if (0 <= grid.row && grid.row < 5 && 0 <= grid.column && grid.column < 5) return grid;
            return (-1, -1);
        }
    }
}