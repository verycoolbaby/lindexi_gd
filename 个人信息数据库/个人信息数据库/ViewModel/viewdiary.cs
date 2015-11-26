﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using 个人信息数据库.model;

namespace 个人信息数据库.ViewModel
{
    public class viewdiary:notify_property
    {
        public viewdiary(viewModel _viewModel,model.model _model)
        {
            this._model = _model;
            this._viewModel = _viewModel;

            _viewModel.diary = this;
        }

        public cdiary diary
        {
            set;
            get;
        } = new cdiary();

        public System.Collections.ObjectModel.ObservableCollection<cdiary> ldiary
        {
            set;
            get;
        } = new System.Collections.ObjectModel.ObservableCollection<cdiary>();
        public System.Windows.Visibility visibility
        {
            set
            {
                _visibility = value;
                OnPropertyChanged();
            }
            get
            {
                return _visibility;
            }
        }
        public new string reminder
        {
            set
            {
                _model.reminder = value;
            }
            get
            {
                return _model.reminder;
            }
        }
        public void add()
        {
            reminder = "添加日记";
        }
        public void delete()
        {
            reminder = "删除日记";
        }

        public void select()
        {
            reminder = "查询日记";
        }

        public void modify()
        {
            reminder = "修改日记";
        }

        public void eliminate()
        {
            reminder = "清除";
        }
        public string warn
        {
            set
            {
                _warn = value;
                OnPropertyChanged();
            }
            get
            {
                return _warn;
            }
        }
        private string _warn="输入";
        private System.Windows.Visibility _visibility = System.Windows.Visibility.Hidden;
        private viewModel _viewModel;

        private model.model _model
        {
            set;
            get;
        }

        
    }
}
