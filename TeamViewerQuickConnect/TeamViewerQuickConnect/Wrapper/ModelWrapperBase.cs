using System;
using System.Runtime.CompilerServices;

namespace QuickConnect
{
    public class ModelWrapperBase<T> : Observable
    {
        public ModelWrapperBase(T model)
        {
            if (model == null)
            {
                throw new ArgumentNullException("model");
            }
            Model = model;
        }

        public T Model { get; private set; }

        protected TValue GetValue<TValue>([CallerMemberName] string propertyName = null)
        {
            var propertyInfo = Model.GetType().GetProperty(propertyName);
            return (TValue)propertyInfo.GetValue(Model);
        }

        protected void SetValue<TValue>(TValue newValue, [CallerMemberName] string propertyName = null)
        {
            var propertyInfo = Model.GetType().GetProperty(propertyName);
            var currentValue = propertyInfo.GetValue(Model);
            if (!Equals(currentValue, newValue))
            {
                propertyInfo.SetValue(Model, newValue);
                OnPropertyChanged(propertyName);
            }
        }

    }
}
