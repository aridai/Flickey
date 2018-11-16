using System;
using System.Windows;
using System.Windows.Controls;

namespace Flickey.Controls
{
    /// <summary>
    /// Interaction logic for Key.xaml
    /// </summary>
    public partial class Key : UserControl
    {
        /// <summary>
        /// Shapeプロパティの依存関係プロパティ。
        /// </summary>
        public static DependencyProperty ShapeProperty
            = DependencyProperty.Register(nameof(Shape), typeof(KeyShape), typeof(Key), new PropertyMetadata(KeyShape.Normal));

        /// <summary>
        /// キーの形を取得・設定します。
        /// </summary>
        public KeyShape Shape
        {
            get => (KeyShape)this.GetValue(ShapeProperty);
            set => this.SetValue(ShapeProperty, value);
        }

        public Key()
        {
            InitializeComponent();
        }
    }
}