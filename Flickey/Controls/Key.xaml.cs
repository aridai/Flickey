using Reactive.Bindings.Extensions;
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
            //  複数回SubscribeされるためHot変換しておく。
            var touchDownStream = Observable.FromEvent<EventHandler<TouchEventArgs>, TouchEventArgs>(
                onNext => (sender, args) => onNext(args),
                handler => this.TouchDown += handler,
                handler => this.TouchDown -= handler).Publish();

            var touchUpStream = Observable.FromEvent<EventHandler<TouchEventArgs>, TouchEventArgs>(
                onNext => (sender, args) => onNext(args),
                handler => this.TouchUp += handler,
                handler => this.TouchUp -= handler).Publish();

            var touchMoveStream = Observable.FromEvent<EventHandler<TouchEventArgs>, TouchEventArgs>(
                onNext => (sender, args) => onNext(args),
                handler => this.PreviewTouchMove += handler,
                handler => this.PreviewTouchMove -= handler).Publish();

            touchDownStream.Connect().AddTo(this.disposable);
            touchUpStream.Connect().AddTo(this.disposable);
            touchMoveStream.Connect().AddTo(this.disposable);

            this.TouchDownAsObservable = touchDownStream;
            this.TouchUpAsObservable = touchUpStream;
            this.TouchMoveAsObservable = touchMoveStream;
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