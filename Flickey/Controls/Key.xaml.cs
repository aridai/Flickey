using System;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Flickey.Controls
{
    using System.Collections.Generic;
    using KeyboardComponents;
    using Reactive.Bindings;
    using Reactive.Bindings.Extensions;

    /// <summary>
    /// Interaction logic for Key.xaml
    /// </summary>
    public partial class Key : UserControl, IDisposable
    {
        private readonly CompositeDisposable disposable = new CompositeDisposable();

        private int? raw;

        private int? column;

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

        //  なんかCharactersSetみたいなやつにまとめたほうが良さみあるかも!?

        public IReadOnlyList<KeyCharacters> Characters { get; }
            = new[]{
                new KeyCharacters(LabelStyle.TwoLines, new[] { "1", "☆", "♪", "→", null }),
                new KeyCharacters(LabelStyle.OneLine, new[] { "A", "B", "C", null, null }),
                new KeyCharacters(LabelStyle.OnlyFirstCaracter, new[] { "あ", "い", "う", "え", "お" })
            };

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
            = DependencyProperty.Register(nameof(LabelStyle), typeof(LabelStyle), typeof(Key), new PropertyMetadata(LabelStyle.OnlyFirstCaracter));

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
        /// KeyboardTypeプロパティの依存関係プロパティ。
        /// </summary>
        public static readonly DependencyProperty KeyboardTypeProperty =
            DependencyProperty.Register(nameof(KeyboardType), typeof(KeyboardType), typeof(Key), new PropertyMetadata(KeyboardType.Number, OnKeyboardTypeChanged));

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
        /// キーボードの種類を取得・設定します。
        /// 依存関係プロパティです。
        /// </summary>
        public KeyboardType KeyboardType
        {
            get => (KeyboardType)this.GetValue(KeyboardTypeProperty);
            set => this.SetValue(KeyboardTypeProperty, value);
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
            pos.Select(p => (type.Value.key, type.Value.type, p))
                .Where(tuple => tuple.type == OperationType.Slide || tuple.type == OperationType.Hold)
                .Subscribe(tuple => this.OnFingerPosChanged(tuple.key, tuple.type, tuple.p))
                .AddTo(this.disposable);

            type.Where(tuple => tuple.type == OperationType.Slide).Subscribe(_ => System.Diagnostics.Debug.WriteLine("すらいど"));
        }

        /// <summary>
        /// リソースの破棄を行います。
        /// </summary>
        public void Dispose()
        {
            this.disposable.Dispose();
        }

        private static void OnKeyboardTypeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var key = (Key)d;
        }

        private void OnTapped()
        {
            this.KeyEffect = KeyEffect.Focused;
            System.Diagnostics.Debug.WriteLine($"OnTapped");
        }

        //  指が離されたとき。
        private void OnInputLeft(Key target, FingerPos pos, Action<string> sender)
        {
            //  印字や形などをリセットする。
            this.Reset();

            //  入力が確定した文字を送信する。
            var grid = this.GetAdjacentGridNums(target, pos);
            if (grid == (this.Row, this.Column)) sender(target.Characters[(int)this.KeyboardType].Characters[(int)pos]);
        }

        //  ホールド操作に入ったとき。
        private void OnHolded(Key target)
        {
            //  操作対象のキーの持つ文字の配列を取得しておく。
            //  この配列は中央・左・上・右・下の順に文字が格納されていて、
            //  nullのときは、ホールド時のキーを表示させない。
            var chars = target.Characters[(int)target.KeyboardType].Characters
                .TakeWhile(character => character != null).ToArray();

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
        //  スライド・ホールド問わずに呼ばれる。
        private void OnFingerPosChanged(Key target, OperationType type, FingerPos pos)
        {
            //  ホールド操作のとき。
            if (type == OperationType.Hold)
            {
                var grid = this.GetAdjacentGridNums(target, pos);
                if (grid == (this.Row, this.Column))
                    this.KeyEffect = KeyEffect.Focused;
            }

            //  スライド操作のとき。
            else
            {
                //  なんかおかしいぞ。
                //  スライド開始直後に呼ばれていない!?

                //  操作対象のキーの持つ文字の配列を取得しておく。
                var chars = target.Characters[(int)target.KeyboardType].Characters
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
                        else this.Reset();
                    }
                }

                //  中心の印字を空にする。
                if (pos != FingerPos.Neutral)
                {
                    target.Shape = KeyShape.Empty;
                }
            }

            //var grid = this.GetAdjacentGridNums(target, pos);
            //if (grid == (this.Row, this.Column))
            //{
            //    System.Diagnostics.Debug.WriteLine($"target:({target.Row},{target.Column}), type:{type}, pos:{pos}");
            //    //  スライド操作のとき。
            //    if (type == OperationType.Slide)
            //    {
            //        //  指がニュートラル以外のとき。
            //        if (pos != FingerPos.Neutral)
            //        {
            //            this.Shape = KeyShape.SlideDown + (int)pos - 1;
            //            target.Shape = KeyShape.Empty;
            //        }

            //        //  指がニュートラルのとき。
            //        else
            //        {
            //            target.Shape = KeyShape.Normal;
            //        }
            //    }

            //    //  ホールド操作のとき。
            //    else
            //    {
            //        this.KeyEffect = KeyEffect.Focused;
            //    }
            //}
        }

        //  隣接したキーのGridの番号を返す。
        //  そのキーが存在しない場合は(-1, -1)を返す。
        private (int, int) GetAdjacentGridNums(Key key, FingerPos pos)
        {
            var row = key.Row;
            var colmn = key.Column;
            (int row, int column) grid;

            switch (pos)
            {
                case FingerPos.Neutral: grid = (row, colmn); break;
                case FingerPos.Top: grid = (row - 1, colmn); break;
                case FingerPos.Bottom: grid = (row + 1, colmn); break;
                case FingerPos.Right: grid = (row, colmn + 1); break;
                case FingerPos.Left: grid = (row, colmn - 1); break;
                default: return (-1, -1);
            }

            if (0 <= grid.row && grid.row < 5 && 0 <= grid.column && grid.column < 5) return grid;
            return (-1, -1);
        }

        //  キーの印字や形をリセットする。
        private void Reset()
        {
            //  形をリセットする。
            this.Shape = KeyShape.Normal;
            this.KeyEffect = KeyEffect.NoEffect;
            this.LabelStyle = this.Characters[(int)this.KeyboardType].LabelStyle;

            //  印字の表示方法に応じて印字を設定する。
            var chars = this.Characters[(int)this.KeyboardType].Characters.Where(c => c != null).ToArray();
            this.PrimaryText = (this.LabelStyle == LabelStyle.OneLine) ? chars.Aggregate(string.Empty, (total, next) => total + next) : chars.First();
            this.SecondaryText = chars.Skip(1).Aggregate(string.Empty, (total, next) => total + next);
        }
    }
}