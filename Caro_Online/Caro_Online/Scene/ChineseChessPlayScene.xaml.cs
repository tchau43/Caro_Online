using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Caro_Online.Helper;

namespace Caro_Online
{
    /// <summary>
    /// Interaction logic for ChineseChessPlayScene.xaml
    /// </summary>
    public partial class ChineseChessPlayScene : Window, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
                handler(this, new PropertyChangedEventArgs(propertyName));
        }

        string _boardImg;
        public string boardImg
        {
            get { return _boardImg; }
            set { _boardImg = value; OnPropertyChanged(); }
        }
        
        public ChineseChessPlayScene()
        {
            InitializeComponent();
            boardImg = ImgSource.chineseChessBoardSrc;
            //boardImg = AppDomain.CurrentDomain.BaseDirectory + $"/imgs/ChineseChess/chineseChessBoard.png";
            this.DataContext = this;
        }
    }

    public class ChineseChessDetail : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
                handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }

}
