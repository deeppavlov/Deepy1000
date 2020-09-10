using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace Deepy1000.Shared.Controls
{
    public sealed partial class AreaHeaderControl : UserControl
    {
        public AreaHeaderControl()
        {
            this.InitializeComponent();
        }

        public string Title
        {
            get { return (string)GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }

        // Using a DependencyProperty as the backing store for welcomeText.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TitleProperty =
                DependencyProperty.Register("Title", typeof(string), typeof(AreaHeaderControl), new PropertyMetadata("Deepy 1000"));

        public string Subtitle
        {
            get { return (string)GetValue(SubtitleProperty); }
            set { SetValue(SubtitleProperty, value); }
        }

        // Using a DependencyProperty as the backing store for welcomeText.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SubtitleProperty =
                DependencyProperty.Register("Subtitle", typeof(string), typeof(AreaHeaderControl), new PropertyMetadata("MOON BASE AI ASSISTANT"));
    }
}
