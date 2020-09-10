using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
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
    public sealed partial class HarvesterStatusItemControl : UserControl
    {
        public HarvesterStatusItemControl()
        {
            this.InitializeComponent();
        }

        public string HarvesterNumber
        {
            get { return (string)GetValue(HarvesterNumberProperty); }
            set { SetValue(HarvesterNumberProperty, value); }
        }

        // Using a DependencyProperty as the backing store for welcomeText.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty HarvesterNumberProperty =
                DependencyProperty.Register("HarvesterNumber", typeof(string), typeof(AreaHeaderControl), new PropertyMetadata("01"));

        public string HarvesterStatus
        {
            get { return (string)GetValue(HarvesterStatusProperty); }
            set { SetValue(HarvesterStatusProperty, value); }
        }

        // Using a DependencyProperty as the backing store for welcomeText.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty HarvesterStatusProperty =
                DependencyProperty.Register("HarvesterStatus", typeof(string), typeof(AreaHeaderControl), new PropertyMetadata("OPTIMAL"));

        public string HarvesterLoad
        {
            get { return (string)GetValue(HarvesterLoadProperty); }
            set { SetValue(HarvesterLoadProperty, value); }
        }

        // Using a DependencyProperty as the backing store for welcomeText.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty HarvesterLoadProperty =
                DependencyProperty.Register("HarvesterLoad", typeof(string), typeof(AreaHeaderControl), new PropertyMetadata("27.5%"));

        public Brush TitleForeground
        {
            get { return (Brush)GetValue(TitleForegroundProperty); }
            set { SetValue(TitleForegroundProperty, value); }
        }

        // Using a DependencyProperty as the backing store for welcomeText.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TitleForegroundProperty =
                DependencyProperty.Register("TitleForeground", typeof(Brush), typeof(AreaHeaderControl), new PropertyMetadata(new SolidColorBrush(Colors.Black)));
    }
}
