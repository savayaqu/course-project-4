using System.Collections.Specialized;
using System.ComponentModel;
using System.Reflection;
using CommunityToolkit.Mvvm.ComponentModel;

namespace PicsyncClient.Attributes;

[AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
public class NotifyPropertyChangedOnCollectionChangeForAttribute : Attribute
{
    public string TargetProperty { get; }

    public NotifyPropertyChangedOnCollectionChangeForAttribute(string targetProperty)
    {
        TargetProperty = targetProperty;
    }
}

public partial class EnhancedObservableObject : ObservableObject
{
    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);

        // Проверяем наличие атрибутов NotifyPropertyChangedOnCollectionChangeFor
        var property = GetType().GetProperty(e.PropertyName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        if (property != null)
        {
            var attributes = property.GetCustomAttributes(typeof(NotifyPropertyChangedOnCollectionChangeForAttribute), true)
                                     .Cast<NotifyPropertyChangedOnCollectionChangeForAttribute>();

            foreach (var attribute in attributes)
            {
                OnPropertyChanged(attribute.TargetProperty);
            }

            if (property.GetValue(this) is INotifyCollectionChanged newCollection)
            {
                newCollection.CollectionChanged += (s, args) =>
                {
                    foreach (var attribute in attributes)
                    {
                        OnPropertyChanged(attribute.TargetProperty);
                    }
                };
            }
        }
    }
}