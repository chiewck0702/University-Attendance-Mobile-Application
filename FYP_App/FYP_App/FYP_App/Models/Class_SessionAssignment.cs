using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace FYP_App.Models
{
    public class Class_SessionAssignment : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        void OnPropertyChanged(string name) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        public int ClassId { get; set; }
        public string ClassName { get; set; }
        public List<LecturerList> LecturerNameList1 { get; set; }

        List<LecturerList> _LecturerNameList2;
        public List<LecturerList> LecturerNameList2 
        { 
            get => _LecturerNameList2;
            set
            {
                if (_LecturerNameList2 != value)
                {
                    _LecturerNameList2 = value;
                    OnPropertyChanged(nameof(LecturerNameList2));
                }
            }
        }

        public LecturerList SelectedLecturer1 { get; set; }
        public string SelectedSessionTypeLecturer1 { get; set; }
        public string LecturerName1 { get; set; }
        public int SelectedLecturerIndex1 { get; set; }
        public LecturerList SelectedLecturer2 { get; set; }
        public string SelectedSessionTypeLecturer2 { get; set; }

        string _LecturerName2;
        public string LecturerName2 
        { 
            get => _LecturerName2; 
            set
            {
                if (_LecturerName2 != value)
                {
                    _LecturerName2 = value;
                    OnPropertyChanged(nameof(LecturerName2));
                }
            }
        }

        public int SelectedLecturerIndex2 { get; set; }


        bool _isEnableSessionOptionRBGroup1;
        public bool isEnableSessionOptionRBGroup1 
        { 
            get => _isEnableSessionOptionRBGroup1; 
            set
            {
                if (_isEnableSessionOptionRBGroup1 != value) // so that do only when the value is different from original 
                {
                    _isEnableSessionOptionRBGroup1 = value;
                    OnPropertyChanged(nameof(isEnableSessionOptionRBGroup1));
                }
            }
        }

        bool _isVisibleLecturerLabel;
        public bool isVisibleLecturerLabel 
        { 
            get => _isVisibleLecturerLabel;
            set
            {
                if (_isVisibleLecturerLabel != value)
                {
                    _isVisibleLecturerLabel = value;
                    OnPropertyChanged(nameof(isVisibleLecturerLabel));
                }
            }
        }

        bool _isVisibleLecturerPicker;
        public bool isVisibleLecturerPicker 
        { 
            get => _isVisibleLecturerPicker; 
            set
            {
                if (_isVisibleLecturerPicker != value)
                {
                    _isVisibleLecturerPicker = value;
                    OnPropertyChanged(nameof(isVisibleLecturerPicker));
                }
            }
        }

        bool _isVisiblelayoutSession2;
        public bool isVisiblelayoutSession2 
        { 
            get => _isVisiblelayoutSession2;
            set
            {
                if (_isVisiblelayoutSession2 != value)
                {
                    _isVisiblelayoutSession2 = value;
                    OnPropertyChanged(nameof(isVisiblelayoutSession2));
                }
            }
        }

        
        public bool isCheckedLecture { get; set; }
        public bool isCheckedLab {  get; set; }
        public bool isCheckedBoth { get; set; } = true;


        bool _isCheckedLecture2;
        public bool isCheckedLecture2 
        {
            get => _isCheckedLecture2;
            set
            {
                if (_isCheckedLecture2 != value)
                {
                    _isCheckedLecture2 = value;
                    OnPropertyChanged(nameof(isCheckedLecture2));
                }
            }
        }

        bool _isCheckedLab2;
        public bool isCheckedLab2
        {
            get => _isCheckedLab2;
            set
            {
                if (_isCheckedLab2 != value)
                {
                    _isCheckedLab2 = value;
                    OnPropertyChanged(nameof(isCheckedLab2));
                }
            }
        }

        public string groupNameRBSession1 => $"RBGroupSession1_{ClassId}";
        public string groupNameRBSession2 => $"RBGroupSession2_{ClassId}";
    }
}
