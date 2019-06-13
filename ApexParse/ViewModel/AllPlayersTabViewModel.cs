using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace ApexParse.ViewModel
{
    class AllPlayersTabViewModel : ViewModelBase
    {
        DamageParser parser;
        MainWindowViewModel parent;
        AllPlayersTabPlayerVM zanversePlayerVM = null;

        public string TabName { get; private set; } = "All";

        public event EventHandler<UserDoubleClickedEventArgs> UserDoubleClickedEvent;

        private ObservableCollection<AllPlayersTabPlayerVM> _allPlayers;
        public ObservableCollection<AllPlayersTabPlayerVM> AllPlayers
        {
            get { return _allPlayers; }
            private set { CallerSetProperty(ref _allPlayers, value); }
        }

        public RelayCommand<AllPlayersTabPlayerVM> UserDoubleClickedCommand { get; private set; }

        private ListCollectionView _allPlayersView;
        public ListCollectionView AllPlayersView
        {
            get { return _allPlayersView; }
            set { CallerSetProperty(ref _allPlayersView, value); }
        }

        public AllPlayersTabViewModel(MainWindowViewModel parentVM)
        {
            parent = parentVM;
            parser = parent.CurrentDamageParser;
            parser.UpdateTick += Parser_UpdateTick;
            UserDoubleClickedCommand = new RelayCommand<AllPlayersTabPlayerVM>(userDoubleClicked);
            AllPlayers = new ObservableCollection<AllPlayersTabPlayerVM>();
            AllPlayersView = createViewSource();
        }

        public void ClearPlayers()
        {
            AllPlayers.Clear();
            zanversePlayerVM = null; //whoops : v)
        }

        public void RefreshState()
        {
            ensureParserState();
            foreach (var playerVM in AllPlayers) playerVM.ParserUpdate();
            AllPlayersView.Refresh(); //only refresh once there are no more pending updates to reduce sort calls
        }

        private void userDoubleClicked(AllPlayersTabPlayerVM playerVM)
        {
            if (playerVM != null)
            {
                UserDoubleClickedEvent?.Invoke(this, new UserDoubleClickedEventArgs(playerVM.AssociatedPlayer));
            }
        }

        private void Parser_UpdateTick(object sender, UpdateTickEventArgs e)
        {
            App.Current.Dispatcher.Invoke(() =>
            {
                RefreshState();
            });
        }

        private ListCollectionView createViewSource()
        {
            var source = CollectionViewSource.GetDefaultView(_allPlayers) as ListCollectionView;
            source.CustomSort = new CustomSortPlayers();
            return source;
        }

        class CustomSortPlayers : IComparer
        {
            public int Compare(object x, object y)
            {
                var first = x as AllPlayersTabPlayerVM;
                var second = y as AllPlayersTabPlayerVM;

                //if we add more special players, this below hack is gonna need changing.
                if (first.Name == "Zanverse") return 1;
                else if (second.Name == "Zanverse") return -1;
                

                int val = second.TotalDamage.CompareTo(first.TotalDamage);
                if (val == 0)
                {
                    return second.Name.CompareTo(first.Name);
                }
                else
                {
                    return val;
                }
            }
        }

        /// <summary>
        /// again, descriptive name
        /// Ensures that AllPlayers variable contains the same number of players as parser.Players does
        /// </summary>
        private void ensureParserState()
        {
            //Add missing players to AllPlayers
            foreach (var curItemKVP in parser.Players)
            {
                if (!AllPlayers.Select(p => p.AssociatedPlayer).Contains(curItemKVP.Value))
                {
                    if (DamageParser.IsBlacklistedUsername(curItemKVP.Value.Name)) continue;
                    AllPlayers.Add(new AllPlayersTabPlayerVM(parent, curItemKVP.Value));
                }
            }

            if (parent.SeparateZanverse)
            {
                if (zanversePlayerVM == null)
                {
                    zanversePlayerVM = new AllPlayersTabPlayerVM(parent, parser.ZanversePlayer);
                    AllPlayers.Add(zanversePlayerVM);
                }
            }
            else if(zanversePlayerVM != null)
            {
                if (AllPlayers.Contains(zanversePlayerVM))
                {
                    AllPlayers.Remove(zanversePlayerVM);
                }
                zanversePlayerVM = null;
            }
        }
    }
}
