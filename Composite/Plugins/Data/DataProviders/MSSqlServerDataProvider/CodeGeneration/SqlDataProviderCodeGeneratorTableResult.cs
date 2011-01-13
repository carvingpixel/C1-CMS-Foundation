using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Linq;
using System.Linq;
using System.Reflection;
using Composite.Data;
using Composite.Data.Plugins.DataProvider;
using Composite.Plugins.Data.DataProviders.MSSqlServerDataProvider.CodeGeneration.Foundation;


namespace Composite.Plugins.Data.DataProviders.MSSqlServerDataProvider.CodeGeneration
{
    internal sealed class SqlDataProviderCodeGeneratorTableResult
    {
        private SqlDataProviderCodeGeneratorResult _sqlDataProviderCodeGeneratorResult;
        private Type _interfaceType;

        private Type listOfInterfaceType = null;

        internal Dictionary<string, StoreInformation> Stores { get; set; }


        internal SqlDataProviderCodeGeneratorTableResult(Type interfaceType)
        {
            _interfaceType = interfaceType;
        }



        internal SqlDataProviderCodeGeneratorResult SqlDataProviderCodeGeneratorResult
        {
            set
            {
                _sqlDataProviderCodeGeneratorResult = value;
            }
        }



        public Type DataIdType
        {
            get
            {
                return GetStorage().DataIdType;
            }
        }



        public IQueryable GetQueryable()
        {
            // TODO: Think about caching
            string storeName = GetStorageName(_interfaceType);

            IQueryable queryable;
            //if (_queryablesCache.TryGetValue(storeName, out queryable) == false)
            {
                if (Stores.ContainsKey(storeName))
                {
                    queryable = SqlDataContextHelperClass.GetTable(_sqlDataProviderCodeGeneratorResult.GetDataContext(),
                                                                   Stores[storeName]);
                }
                else
                {
                    if (listOfInterfaceType == null)
                    {
                        listOfInterfaceType = typeof(List<>);
                        listOfInterfaceType = listOfInterfaceType.MakeGenericType(_interfaceType);                    
                    }

                    IEnumerable list = (IEnumerable)Activator.CreateInstance(listOfInterfaceType, null);

                    return list.AsQueryable();
                }

                //_queryablesCache.Add(storeName, queryable);
            }

            return queryable;
        }        



        public IData GetDataByDataId(IDataId dataId, DataProviderContext dataProivderContext)
        {
            var storage = GetStorage();

            Verify.That(storage.DataIdType == dataId.GetType(), "Only data ids from this provider is allowed to be deleted with the provider");

            return storage.SqlDataProviderHelperMethods.GetDataById(GetQueryable(), dataId, dataProivderContext);
        }


        public IData AddNew(IData dataToAdd, DataProviderContext dataProivderContext, DataContext dataContext)
        {
            return GetStorage().SqlDataProviderHelperMethods.AddData((ISqlDataContext)dataContext, dataToAdd, dataProivderContext);
        }

        public void RemoveData(IData dataToRemove, DataContext dataContext)
        {
            GetStorage().SqlDataProviderHelperMethods.RemoveData((ISqlDataContext)dataContext, dataToRemove);
        }

        private StoreInformation GetStorage()
        {
            string storeName = GetStorageName(_interfaceType);
            Verify.That(Stores.ContainsKey(storeName), "The store named '{0}' is not supported for type '{1}'", storeName, _interfaceType);

            return Stores[storeName];
        }

        private static string GetStorageName(Type interfaceType)
        {
            string dataScope = DataScopeManager.MapByType(interfaceType).Name;
            string cultureInfo = LocalizationScopeManager.MapByType(interfaceType).Name;
            return Composite.Plugins.Data.DataProviders.MSSqlServerDataProvider.SqlDataProvider.GetStorageName(dataScope, cultureInfo);
        }



        internal sealed class StoreInformation
        {
            internal FieldInfo DataContextQueryableFieldInfo { get; set; }
            internal ISqlDataProviderHelperMethods SqlDataProviderHelperMethods { get; set; }
            internal Type DataIdType { get; set; }
        }              
    }
}
