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

namespace Caro_Online
{
    /// <summary>
    /// Interaction logic for MainScene.xaml
    /// </summary>
    public partial class MainScene : Window
    {
        public MainScene()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            CaroPlayScene caroPlayScene = new CaroPlayScene();
            caroPlayScene.Show();
            this.Close();

            //NavigationService.Navigate(new CaroPlayScene());
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            ChineseChessPlayScene scene = new ChineseChessPlayScene();
            scene.Show();
            this.Close();
        }
    }
}
