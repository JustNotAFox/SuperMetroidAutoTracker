using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
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
using System.Threading;

namespace AutoTracker
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        MemoryReader memAccess;
        Trackables tracked;
        public MainWindow()
        {
            tracked = new Trackables();
            this.DataContext = tracked;
            InitializeComponent();
            ThreadStart method = new ThreadStart(dataUpdater);
            Thread updater = new Thread(method);
            updater.Start();
        }
        private void dataUpdater()
        {
            memAccess = new MemoryReader();
            byte[] buffer = new byte[2];
            while (true)
            {
                int check = memAccess.getBytes(0x09A4, buffer, 2);
                if (check == 2)
                {
                    tracked.varia = (buffer[0] & 0x1) != 0;
                    tracked.spring = (buffer[0] & 0x2) != 0;
                    tracked.morph = (buffer[0] & 0x4) != 0;
                    tracked.screw = (buffer[0] & 0x8) != 0;
                    tracked.grav = (buffer[0] & 0x20) != 0;
                    tracked.hjb = (buffer[1] & 0x1) != 0;
                    tracked.space = (buffer[1] & 0x2) != 0;
                    tracked.bomb = (buffer[1] & 0x10) != 0;
                    tracked.speed = (buffer[1] & 0x20) != 0;
                }
                check = memAccess.getBytes(0x09A8, buffer, 2);
                if (check == 2)
                {
                    tracked.wave = (buffer[0] & 0x1) != 0;
                    tracked.ice = (buffer[0] & 0x2) != 0;
                    tracked.spazer = (buffer[0] & 0x4) != 0;
                    tracked.plasma = (buffer[0] & 0x8) != 0;
                    tracked.charge = (buffer[1] & 0x10) != 0;
                }
                Thread.Sleep(1000);
            }
        }
    }

    public class Trackables : INotifyPropertyChanged
    {
        private bool _varia;
        public bool varia
        {
            get { return this._varia; }
            set { Set(value, ref this._varia, "varia"); }
        }
        private bool _spring;
        public bool spring
        {
            get { return this._spring; }
            set { Set(value, ref this._spring, "spring"); }
        }
        private bool _morph;
        public bool morph
        {
            get { return this._morph; }
            set { Set(value, ref this._morph, "morph"); }
        }
        private bool _screw;
        public bool screw
        {
            get { return this._screw; }
            set { Set(value, ref this._screw, "screw"); }
        }
        private bool _grav;
        public bool grav
        {
            get { return this._grav; }
            set { Set(value, ref this._grav, "grav"); }
        }
        private bool _hjb;
        public bool hjb
        {
            get { return this._hjb; }
            set { Set(value, ref this._hjb, "hjb"); }
        }
        private bool _space;
        public bool space
        {
            get { return this._space; }
            set { Set(value, ref this._space, "space"); }
        }
        private bool _bomb;
        public bool bomb {
            get { return this._bomb; }
            set { Set(value, ref this._bomb, "bomb"); }
        }
        private bool _speed;
        public bool speed
        {
            get { return this._speed; }
            set { Set(value, ref this._speed, "speed"); }
        }
        private bool _grapple;
        public bool grapple
        {
            get { return this._grapple; }
            set { Set(value, ref this._grapple, "grapple"); }
        }
        private bool _xray;
        public bool xray
        {
            get { return this._xray; }
            set { Set(value, ref this._xray, "xray"); }
        }
        private bool _charge;
        public bool charge
        {
            get { return this._charge; }
            set { Set(value, ref this._charge, "charge"); }
        }
        private bool _spazer;
        public bool spazer
        {
            get { return this._spazer; }
            set { Set(value, ref this._spazer, "spazer"); }
        }
        private bool _ice;
        public bool ice
        {
            get { return this._ice; }
            set { Set(value, ref this._ice, "ice"); }
        }
        private bool _wave;
        public bool wave
        {
            get { return this._wave; }
            set { Set(value, ref this._wave, "wave"); }
        }
        private bool _plasma;
        public bool plasma
        {
            get { return this._plasma; }
            set { Set(value, ref this._charge, "plasma"); }
        }
        public event PropertyChangedEventHandler PropertyChanged;
        private void RaisePropertyChanged(string propertyName)
        {
            if(this.PropertyChanged != null)
            {
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        public Trackables()
        {
            _varia = false;
            _spring = false;
            _morph = false;
            _screw = false;
            _grav = false;
            _hjb = false;
            _space = false;
            _bomb = false;
            _speed = false;
            _grapple = false;
            _xray = false;
            _charge = false;
            _spazer = false;
            _ice = false;
            _wave = false;
            _plasma = false;
        }
        private void Set(bool value, ref bool cur, string propName)
        {
            if(value != cur)
            {
                cur = value;
                RaisePropertyChanged(propName);
            }
        }
    }
}