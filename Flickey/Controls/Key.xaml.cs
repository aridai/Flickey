using System;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Flickey.Controls
{
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
        /// キーの形を取得・設定します。
        /// 依存関係プロパティです。
        /// </summary>
        public KeyShape Shape
        {
            get => (KeyShape)this.GetValue(ShapeProperty);
            set => this.SetValue(ShapeProperty, value);
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