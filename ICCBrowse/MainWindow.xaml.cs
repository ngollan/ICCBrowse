using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Win32;
using System.Diagnostics;

namespace ICCBrowse {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        public MainWindow() {
            InitializeComponent();
        }

        private void MenuQuit_Click(object sender, RoutedEventArgs e) {
            Application.Current.Shutdown();
        }

        private void MenuOpenFile_Click(object sender, RoutedEventArgs e) {
            OpenFileDialog open = new OpenFileDialog();
            Nullable<bool> result = open.ShowDialog();

            if (true == result) {
                Debug.WriteLine($"Opening {open.FileName}");
                var profile = new ICCv4File(open.FileName);
                Debug.WriteLine(profile.ToString);

                foreach(var stub in profile.TagStubs) {
                    Debug.WriteLine($" * {stub.ToString}");
                }
            }
        }
    }
}
