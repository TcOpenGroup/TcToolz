namespace Libs.Update
{


    public class UpdateModel : Update
    {
        public UpdateModel()
        {
            ExecuteUpdate();
        }

        //private ICommand updateCommand;
        //public ICommand UpdateCommand
        //{
        //    get
        //    {
        //        return this.updateCommand = new DelegateCommand(this.ExecuteUpdate);
        //    }
        //}
        public void ExecuteUpdate()
        {
            UpdateTask();
        }
    }
}
