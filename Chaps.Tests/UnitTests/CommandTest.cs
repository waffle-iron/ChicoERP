using Microsoft.VisualStudio.TestTools.UnitTesting;
using Chaps.Command;
using System.Windows.Input;
using System;
using Chaps.MVVM;

namespace UmzugManager.MVVMLib.Tests.UnitTests
{
    [TestClass]
    public class CommandTest
    {

        [TestMethod]
        public void IsCommandExecuted()
        {
            bool executed = false;
            ICommand command = new RelayCommand(() => { executed = true;  });

            command.Execute(null);

            Assert.IsTrue(executed);
        }

        [TestMethod]
        public void IsCanExecuteCalledAndReturnsRightValue()
        {
            bool canExCalled = false;
            RelayCommand command = new RelayCommand(() => { }, () => { canExCalled = true; return true; });

            command.CanExecute();
            
            Assert.IsTrue(canExCalled);
        }

        [TestMethod]
        public void IsCanExecuteCalledOnPropertyChanged()
        {
            DummieClass testClass = new DummieClass();

            testClass.DummieValue = 25;
            
            Assert.IsTrue(testClass.canExCalled);
        }
        

        [TestMethod]
        public void IsCanExecuteChangedCalled()
        {
            bool isCalled = false;
            ICommand command = new RelayCommand(() => { });

            command.CanExecuteChanged += (object s, System.EventArgs e) => { isCalled = true; };

            ((RelayCommand)command).RaiseCanExecuteChanged();

            Assert.IsTrue(isCalled);
        }
    }

    class DummieClass: BindableBase
    {
        RelayCommand command;
        public bool canExCalled = false;
        public DummieClass()
        {
            command = new RelayCommand(() => { }, () => {
                canExCalled = true; return true; }).ObservesProperty(() => DummieValue);

            command.CanExecuteChanged += Command_CanExecuteChanged;
        }

        private void Command_CanExecuteChanged(object sender, EventArgs e)
        {
            command.CanExecute();
        }

        private int dummieValue = 0;
        public int DummieValue {
            get
            {
                return dummieValue;
            }
            set
            {
                SetProperty(ref dummieValue, value);
            }
        }


    }
}
