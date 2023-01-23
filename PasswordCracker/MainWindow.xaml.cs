using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
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

namespace PasswordCracker
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private CancellationTokenSource _token = new();
        private static double _counter = 0;
        private static double _maxIterations = 0;
        public MainWindow()
        {
            InitializeComponent();
        }

        private async void BtnCrackPassword_OnClick(object sender, RoutedEventArgs e)
        {
            _token = new CancellationTokenSource();
            barProgress.Value = 0;
            _counter = 0;
            var prefixes = Cracker.GetPrefixes(txtAlphabet.Text, 2);

            _maxIterations = Math.Pow(txtAlphabet.Text.Length, int.Parse(txtLength.Text));
            Console.WriteLine($"Max. Iterations: {_maxIterations}");
            var progress = new Progress<double>();
            progress.ProgressChanged += (_, x) =>
            {
                _counter += x;
                var val = (_counter / _maxIterations) * 100;
                barProgress.Value = val;
                if (val <= 100)
                {
                    lblStatus.Content = $"{val:F0}%";
                }
            };
            var response = await new Cracker().MultiCrackAsync(txtHash.Text,
                txtAlphabet.Text,
                int.Parse(txtLength.Text) - 1,
                progress,
                4,
                _token.Token);
            var res = await response;
            if (res == "")
            {
                lblStatus.Content = "[ERROR] Password not cracked!";
                barProgress.Value = 0;
            }
            else
            {
                _token.Cancel();
                lblStatus.Content = res;
                barProgress.Value = 100;
            }
        }

        private void BtnStop_OnClick(object sender, RoutedEventArgs e)
        {
            barProgress.Value = 0;
            lblStatus.Content = "";
            _token.Cancel();
        }
        
    }
}