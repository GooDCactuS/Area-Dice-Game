using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SemWork
{
    public class SquareData : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private string color;
        public int Row { get; set; }
        public int Column { get; set; }
        public string Color { get { return color; } set { color = value; RaisePropertyChanged("Color"); } }

        protected virtual void RaisePropertyChanged(PropertyChangedEventArgs e)
        {
            var handler = PropertyChanged;
            if (handler != null) handler(this, e);
        }

        protected void RaisePropertyChanged(string propertyName)
        {
            RaisePropertyChanged(new PropertyChangedEventArgs(propertyName));
        }
    }
}
