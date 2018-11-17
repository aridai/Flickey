using System;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Flickey.Controls
{
    using KeyboardComponents;

    /// <summary>
    /// Interaction logic for Key.xaml
    /// </summary>
    public partial class Key : UserControl, IDisposable
    {
        private readonly CompositeDisposable disposable = new CompositeDisposable();

        private int? raw, column;

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
        /// リソースの破棄を行います。
        /// </summary>
        public void Dispose()
        {
            this.disposable.Dispose();
        }
    }
}