using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using WZWVAPI;

namespace WZWVAPI
{
    public enum OrderBy { ASC, DESC }

    public abstract class DataHandler
    {
        protected string tableName { get; set; }
        protected Field[] fields { get; set; }
        protected Type objectType;
        protected string[] customQueries;
        protected bool LogDatabaseStats = true;
        protected DataObject DefaultDataObject = null;

        protected DataHandler(string tableName, Field[] fields, Type objectType, string[] customQueries = null)
        {
            this.tableName = tableName;
            List<Field> fieldsList = fields.ToList();
            fieldsList.Insert(0, new Field("ID", typeof(int), 0, true));
            this.fields = fieldsList.ToArray();
            this.objectType = objectType;
            this.customQueries = customQueries;
        }

        private List<DataObject> GetDataObjects(MySqlDataReader Reader)
        {
            List<DataObject> ObjectList = new List<DataObject>();
            List<object> Parameters = new List<object>();

            if (Reader.HasRows)
            {
                try
                {
                    while (Reader.Read())
                    {
                        Parameters = new List<object>();

                        foreach (Field f in fields)
                        {
                            if (f.FieldType == typeof(int))
                            {
                                Parameters.Add(Convert.ToInt32(Reader[f.FieldName]));
                                continue;
                            }

                            if (f.FieldType == typeof(string))
                            {
                                Parameters.Add(Convert.ToString(Reader[f.FieldName]));
                                continue;
                            }

                            if (f.FieldType == typeof(bool))
                            {
                                Parameters.Add(Convert.ToBoolean(Reader[f.FieldName]));
                                continue;
                            }
                        }

                        ObjectList.Add((DataObject)Activator.CreateInstance(this.objectType, Parameters.ToArray()));
                    }
                }
                catch (InvalidCastException)
                {
                    //Row is empty
                }
                catch (Exception e)
                {
                    DatabaseHandler.CloseConnectionByReader(Reader);
                    throw e;
                }
            }

            DatabaseHandler.CloseConnectionByReader(Reader);

            return ObjectList;
        }

        private bool CreateTable()
        {
            try
            {
                Field PrimaryKey = null;

                MySqlCommand Command = new MySqlCommand();
                Command.CommandText = "CREATE TABLE IF NOT EXISTS " + tableName + " ( ";
                foreach (Field f in fields)
                {
                    if (f.FieldType == typeof(int))
                    {
                        Command.CommandText += " " + f.FieldName + " INT NOT NULL " + (f.Key ? " AUTO_INCREMENT" : "") + " ,";

                        if (f.Key)
                        {
                            if (PrimaryKey == null)
                            {
                                PrimaryKey = f;
                            }
                            else
                            {
                                throw new Exception("Instance has 2 primary keys");
                            }
                        }
                    }

                    if (f.FieldType == typeof(string))
                    {
                        Command.CommandText += " " + f.FieldName + " VARCHAR(" + f.Size + ") NOT NULL ,";
                    }

                    if (f.FieldType == typeof(bool))
                    {
                        Command.CommandText += " " + f.FieldName + " BIT NOT NULL ,";
                    }
                }

                if (PrimaryKey != null)
                {
                    Command.CommandText += "PRIMARY KEY (" + PrimaryKey.FieldName + "), " +
                        "UNIQUE KEY ID_UNIQUE (" + PrimaryKey.FieldName + ") );";
                }
                else
                {
                    Command.CommandText = Command.CommandText.Substring(0, Command.CommandText.Length - 2);
                    Command.CommandText += " );";
                }

                DatabaseHandler.ExecuteNonQuery(Command, this.LogDatabaseStats);

                if (this.DefaultDataObject != null)
                {
                    this.AddObject(this.DefaultDataObject);
                }

                return true;
            }
            catch (Exception e)
            {
                if (e.Message.Contains("Multiple primary key defined"))
                {
                    return false;
                }

                exceptionHandler(e);
                return false;
            }
        }

        private bool RestructureTable()
        {
            Field PrimaryKey = null;

            foreach (Field f in this.fields)
            {
                try
                {
                    MySqlCommand Command = new MySqlCommand();
                    if (f.FieldType == typeof(int))
                    {
                        Command.CommandText = "ALTER TABLE " + tableName + " ADD COLUMN " + f.FieldName + " INT NOT NULL " + (f.Key ? " AUTO_INCREMENT" : "");

                        if (f.Key)
                        {
                            if (PrimaryKey == null)
                            {
                                PrimaryKey = f;
                            }
                            else
                            {
                                throw new Exception("Instance has 2 primary keys");
                            }
                        }
                    }

                    if (f.FieldType == typeof(string))
                    {
                        Command.CommandText = "ALTER TABLE " + tableName + " ADD COLUMN " + f.FieldName + " VARCHAR(" + f.Size + ") NOT NULL ";
                    }

                    if (f.FieldType == typeof(bool))
                    {
                        Command.CommandText = "ALTER TABLE " + tableName + " ADD COLUMN " + f.FieldName + " BIT NOT NULL ";
                    }

                    DatabaseHandler.ExecuteNonQuery(Command, this.LogDatabaseStats);
                }
                catch (Exception e)
                {
                    if (e.Message.StartsWith("Duplicate column name "))
                    {
                        try
                        {
                            MySqlCommand Command = new MySqlCommand();
                            if (f.FieldType == typeof(int))
                            {
                                Command.CommandText = "ALTER TABLE " + tableName + " CHANGE COLUMN " + f.FieldName + " " + f.FieldName + " INT NOT NULL " + (f.Key ? " AUTO_INCREMENT" : "");

                                if (f.Key && PrimaryKey == null)
                                {
                                    PrimaryKey = f;
                                }
                                else
                                {
                                    if (PrimaryKey != f && f.Key)
                                    {
                                        throw new Exception("Instance has 2 primary keys");
                                    }
                                }

                            }

                            if (f.FieldType == typeof(string))
                            {
                                Command.CommandText = "ALTER TABLE " + tableName + " CHANGE COLUMN " + f.FieldName + " " + f.FieldName + " VARCHAR(" + f.Size + ") NOT NULL ";
                            }

                            if (f.FieldType == typeof(bool))
                            {
                                Command.CommandText = "ALTER TABLE " + tableName + " CHANGE COLUMN " + f.FieldName + " " + f.FieldName + " BIT NOT NULL ";
                            }

                            DatabaseHandler.ExecuteNonQuery(Command, this.LogDatabaseStats);
                        }
                        catch (Exception)
                        {
                            return false;
                        }
                    }
                    else if (e.Message.Contains("Multiple primary key defined"))
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }

            if (PrimaryKey != null)
            {
                try
                {
                    MySqlCommand Command = new MySqlCommand();
                    Command.CommandText = "alter TABLE " + this.tableName + " ADD PRIMARY KEY (`" + PrimaryKey.FieldName + "`);";
                    DatabaseHandler.ExecuteNonQuery(Command, this.LogDatabaseStats);
                }
                catch (Exception ex)
                {
                    if (ex.Message.Contains("Multiple"))
                    {
                        return true;
                    }

                    if (!(this is IDoNotRegisterError))
                    {
                        new WebsiteException(ex);
                    }

                    return false;
                }
            }

            return true;
        }

        public DataObject AddObject(DataObject dataObject)
        {
            try
            {
                MySqlCommand Command = new MySqlCommand();
                Command.CommandText = "INSERT INTO " + tableName + " ( ";
                foreach (Field f in this.fields)
                {
                    if (f.FieldName != "ID")
                    {
                        Command.CommandText += f.FieldName + ", ";
                    }
                }

                Command.CommandText = Command.CommandText.Substring(0, Command.CommandText.Length - 2);
                Command.CommandText += ") VALUES ( ";

                foreach (Field f in this.fields)
                {
                    if (f.FieldName != "ID")
                    {
                        Command.CommandText += " @" + f.FieldName + ", ";
                    }
                }

                Command.CommandText = Command.CommandText.Substring(0, Command.CommandText.Length - 2);
                Command.CommandText += " )";

                for (int i = 0; i < fields.Count(); i++)
                {
                    if (fields[i].FieldName != "ID")
                    {
                        Command.Parameters.AddWithValue("@" + fields[i].FieldName, dataObject.GetType().GetProperty(fields[i].FieldName).GetValue(dataObject, null));
                    }
                }

                DatabaseHandler.ExecuteNonQuery(Command, this.LogDatabaseStats);

                if (!(this is IDoNotRegisterError))
                {
                    FeedbackHandler.AddMessage("Object " + dataObject.ToString() + " with id " + Command.LastInsertedId + " has been added.");
                }

                dataObject.setID(Convert.ToInt32(Command.LastInsertedId));
                CacheHandler.ClearData(this.ToString());
                return dataObject;
            }
            catch (Exception e)
            {
                if (this.exceptionHandler(e))
                {
                    return this.AddObject(dataObject);
                }
                else
                {
                    return null;
                }

            }
        }

        public DataObject UpdateObject(DataObject dataObject)
        {
            IList<PropertyInfo> props = new List<PropertyInfo>(this.objectType.GetProperties());

            if (dataObject.ID == 0)
            {
                throw new Exception("Value 0 is not allowed.");
            }

            try
            {
                MySqlCommand Command = new MySqlCommand();
                Command.CommandText = "UPDATE " + this.tableName + " SET ";

                foreach (Field f in this.fields)
                {
                    if (f.FieldName != "ID")
                    {
                        Command.CommandText += f.FieldName + " = @" + f.FieldName + ", ";

                        foreach (PropertyInfo pi in props)
                        {
                            if (pi.Name == f.FieldName)
                            {
                                Command.Parameters.AddWithValue("@" + f.FieldName, pi.GetValue(dataObject, null));
                                break;
                            }
                        }
                    }
                }

                Command.CommandText = Command.CommandText.Substring(0, Command.CommandText.Length - 2);

                Command.CommandText += " WHERE ID = @ID";
                Command.Parameters.AddWithValue("ID", dataObject.ID);

                DatabaseHandler.ExecuteNonQuery(Command, this.LogDatabaseStats);

                if (!(this is IDoNotRegisterError))
                {
                    FeedbackHandler.AddMessage("Object " + dataObject.ToString() + " with id " + dataObject.ID + " has been updated.");
                }

                CacheHandler.ClearData(this.ToString());

                return dataObject;
            }
            catch (Exception e)
            {
                if (this.exceptionHandler(e))
                {
                    return this.UpdateObject(dataObject);
                }
                else
                {
                    return null;
                }
            }
        }

        public DataObject GetObjectByID(int ID)
        {
            try
            {
                DataObject dataObject = (DataObject)CacheHandler.GetData(this.ToString(), "GetObjectByID", new string[] { Convert.ToString(ID) });

                if (dataObject != null)
                {
                    return dataObject;
                }

                MySqlCommand Command = new MySqlCommand();
                Command.CommandText = "SELECT * FROM " + this.tableName + " WHERE ID = @ID";
                Command.Parameters.AddWithValue("@ID", ID);

                List<DataObject> ObjectList = GetDataObjects(DatabaseHandler.ExecuteQuery(Command, this.LogDatabaseStats));
                QueryTrace.RemoveQuery(Command.CommandText);

                if (ObjectList.Count > 0)
                {
                    CacheHandler.AddToCache(this.ToString(), "GetObjectByID", new string[] { Convert.ToString(ID) }, ObjectList[0]);
                    return ObjectList[0];
                }
                else
                {
                    return null;
                }
            }
            catch (Exception e)
            {
                if (this.exceptionHandler(e))
                {
                    return this.GetObjectByID(ID);
                }
                else
                {
                    return null;
                }
            }
        }

        protected List<DataObject> GetObjectByFieldsAndSearchQuery(Field[] SearchFields, string SearchQuery, bool Exact = false, int LIMIT = 0, OrderBy orderBy = OrderBy.ASC, Field orderByField = null)
        {
            List<string> ParameterList = new List<string>();

            foreach (Field f in fields)
            {
                ParameterList.Add(f.FieldName);
            }

            ParameterList.Add(SearchQuery);

            ParameterList.Add(Exact.ToString());
            ParameterList.Add(LIMIT.ToString());
            ParameterList.Add(orderBy.ToString());

            try
            {
                if (orderByField != null)
                {
                    ParameterList.Add(orderByField.FieldName);
                }
            }
            catch (Exception)
            {

            }

            List<DataObject> dataObjects = (List<DataObject>)CacheHandler.GetData(this.ToString(), "GetObjectByFieldsAndSearchQuery", ParameterList.ToArray());

            if (dataObjects != null)
            {
                return dataObjects;
            }

            MySqlCommand Command = new MySqlCommand();
            string SQLQuery = string.Empty;


            if (!Exact)
            {
                SQLQuery = "%" + SearchQuery + "%";
            }
            else
            {
                SQLQuery = SearchQuery;
            }

            try
            {
                Command.CommandText = "SELECT * FROM " + this.tableName + " WHERE ";

                foreach (Field f in SearchFields)
                {
                    Command.CommandText += f.FieldName + " LIKE @QUERY OR ";
                }

                Command.Parameters.AddWithValue("@QUERY", SQLQuery);

                if (SearchFields.Count() > 0)
                {
                    Command.CommandText = Command.CommandText.Substring(0, Command.CommandText.Length - 3);
                }

                Command.CommandText = this.addOrderBy(Command.CommandText, orderBy, orderByField);

                if (LIMIT > 0)
                {
                    Command.CommandText = this.addLimit(Command.CommandText, LIMIT);
                }

                List<DataObject> dataObjectList = this.GetDataObjects(DatabaseHandler.ExecuteQuery(Command, this.LogDatabaseStats));
                QueryTrace.RemoveQuery(Command.CommandText);

                CacheHandler.AddToCache(this.ToString(), "GetObjectByFieldsAndSearchQuery", ParameterList.ToArray(), dataObjectList);
                return dataObjectList;
            }
            catch (Exception e)
            {
                if (this.exceptionHandler(e))
                {
                    return this.GetObjectByFieldsAndSearchQuery(SearchFields, SearchQuery, Exact, LIMIT, orderBy, orderByField);
                }
                else
                {
                    return new List<DataObject>();
                }
            }
        }

        public List<DataObject> GetObjectsByChildObjectID(Field Child, int ID, int LIMIT, OrderBy orderBy = OrderBy.ASC, Field orderByField = null)
        {
            List<string> ParameterList = new List<string>();

            ParameterList.Add(Child.FieldName);
            ParameterList.Add(ID.ToString());
            ParameterList.Add(LIMIT.ToString());
            ParameterList.Add(orderBy.ToString());

            try
            {
                if (orderByField != null)
                {
                    ParameterList.Add(orderByField.FieldName);
                }
            }
            catch (Exception)
            {

            }

            List<DataObject> dataObjects = (List<DataObject>)CacheHandler.GetData(this.ToString(), "GetObjectsByChildObjectID", ParameterList.ToArray());

            if (dataObjects != null)
            {
                return dataObjects;
            }


            MySqlCommand Command = new MySqlCommand();

            try
            {
                Command.CommandText = "SELECT * FROM " + this.tableName + " WHERE " + Child.FieldName + " = @" + Child.FieldName;
                Command.Parameters.AddWithValue("@" + Child.FieldName, ID);

                Command.CommandText = this.addOrderBy(Command.CommandText, orderBy, orderByField);

                if (LIMIT > 0)
                {
                    Command.CommandText = this.addLimit(Command.CommandText, LIMIT);
                }

                List<DataObject> dataObjectList = this.GetDataObjects(DatabaseHandler.ExecuteQuery(Command, this.LogDatabaseStats));
                QueryTrace.RemoveQuery(Command.CommandText);

                CacheHandler.AddToCache(this.ToString(), "GetObjectsByChildObjectID", ParameterList.ToArray(), dataObjectList);
                return dataObjectList;
            }
            catch (Exception e)
            {
                if (this.exceptionHandler(e))
                {
                    return this.GetObjectsByChildObjectID(Child, ID, LIMIT, orderBy, orderByField);
                }
                else
                {
                    return new List<DataObject>();
                }
            }
        }

        protected List<DataObject> GetObjectList(int LIMIT = 0, OrderBy orderBy = OrderBy.ASC, Field orderByField = null)
        {
            List<string> ParameterList = new List<string>();

            ParameterList.Add(LIMIT.ToString());
            ParameterList.Add(orderBy.ToString());

            try
            {
                if (orderByField != null)
                {
                    ParameterList.Add(orderByField.FieldName);
                }
            }
            catch (Exception)
            {

            }

            List<DataObject> dataObjects = (List<DataObject>)CacheHandler.GetData(this.ToString(), "GetObjectList", ParameterList.ToArray());

            if (dataObjects != null)
            {
                return dataObjects;
            }

            try
            {
                MySqlCommand Command = new MySqlCommand();
                Command.CommandText = "SELECT * FROM " + this.tableName;

                Command.CommandText = this.addOrderBy(Command.CommandText, orderBy, orderByField);

                if (LIMIT > 0)
                {
                    Command.CommandText = this.addLimit(Command.CommandText, LIMIT);
                }

                List<DataObject> dataObjectList = this.GetDataObjects(DatabaseHandler.ExecuteQuery(Command, this.LogDatabaseStats));
                QueryTrace.RemoveQuery(Command.CommandText);

                CacheHandler.AddToCache(this.ToString(), "GetObjectList", ParameterList.ToArray(), dataObjectList);
                return dataObjectList;
            }
            catch (Exception e)
            {
                if (this.exceptionHandler(e))
                {
                    return this.GetObjectList(LIMIT, orderBy, orderByField);
                }
                else
                {
                    return new List<DataObject>();
                }
            }
        }

        public List<DataObject> GetObjectsByIDArray(int[] ID)
        {
            List<string> ParameterList = new List<string>();

            foreach (int i in ID)
            {
                ParameterList.Add(i.ToString());
            }

            List<DataObject> dataObjects = (List<DataObject>)CacheHandler.GetData(this.ToString(), "GetObjectsByIDArray", ParameterList.ToArray());

            if (dataObjects != null)
            {
                return dataObjects;
            }

            try
            {
                MySqlCommand Command = new MySqlCommand();
                Command.CommandText = "SELECT * FROM " + this.tableName + " WHERE ";

                for (int i = 0; i < ID.Length; i++)
                {
                    Command.CommandText += "ID = @ID" + i + " OR ";
                    Command.Parameters.AddWithValue("@ID" + i, ID[i]);
                }

                Command.CommandText = Command.CommandText.Substring(0, Command.CommandText.Length - 3);

                List<DataObject> dataObjectList = this.GetDataObjects(DatabaseHandler.ExecuteQuery(Command, this.LogDatabaseStats));
                QueryTrace.RemoveQuery(Command.CommandText);

                CacheHandler.AddToCache(this.ToString(), "GetObjectsByIDArray", ParameterList.ToArray(), dataObjectList);
                return dataObjectList;
            }
            catch (Exception e)
            {
                if (this.exceptionHandler(e))
                {
                    return this.GetObjectsByIDArray(ID);
                }
                else
                {
                    return null;
                }
            }
        }

        protected List<DataObject> GetObjectsByChildIDArray(Field Child, int[] ID, int LIMIT, OrderBy orderBy = OrderBy.ASC, Field orderByField = null)
        {
            List<string> ParameterList = new List<string>();

            ParameterList.Add(Child.FieldName);

            foreach (int i in ID)
            {
                ParameterList.Add(i.ToString());
            }

            ParameterList.Add(LIMIT.ToString());
            ParameterList.Add(orderBy.ToString());

            try
            {
                if (orderByField != null)
                {
                    ParameterList.Add(orderByField.FieldName);
                }
            }
            catch (Exception)
            {

            }

            List<DataObject> dataObjects = (List<DataObject>)CacheHandler.GetData(this.ToString(), "GetObjectsByChildIDArray", ParameterList.ToArray());

            if (dataObjects != null)
            {
                return dataObjects;
            }


            MySqlCommand Command = new MySqlCommand();

            try
            {
                Command.CommandText = "SELECT * FROM " + this.tableName + " WHERE ";

                for (int i = 0; i < ID.Length; i++)
                {
                    Command.CommandText += Child.FieldName + " = @" + Child.FieldName + i + " OR ";
                    Command.Parameters.AddWithValue("@" + Child.FieldName + i, ID[i]);
                }

                Command.CommandText = Command.CommandText.Substring(0, Command.CommandText.Length - 3);

                Command.CommandText = this.addOrderBy(Command.CommandText, orderBy, orderByField);

                if (LIMIT > 0)
                {
                    Command.CommandText = this.addLimit(Command.CommandText, LIMIT);
                }

                List<DataObject> dataObjectList = this.GetDataObjects(DatabaseHandler.ExecuteQuery(Command, this.LogDatabaseStats));
                QueryTrace.RemoveQuery(Command.CommandText);

                CacheHandler.AddToCache(this.ToString(), "GetObjectsByChildIDArray", ParameterList.ToArray(), dataObjectList);
                return dataObjectList;
            }
            catch (Exception e)
            {
                if (this.exceptionHandler(e))
                {
                    return this.GetObjectsByChildIDArray(Child, ID, LIMIT, orderBy, orderByField);
                }
                else
                {
                    return new List<DataObject>();
                }
            }
        }

        protected List<DataObject> CustomQuery(int QueryIndex, string[] ParameterNames, object[] Parameters)
        {
            List<string> ParameterList = new List<string>();

            try
            {
                ParameterList.Add(QueryIndex.ToString());

                foreach (object o in Parameters)
                {
                    ParameterList.Add(o.ToString());
                }
            }
            catch (Exception e)
            {
                if (!(this is IDoNotRegisterError))
                {
                    new WebsiteException(e);
                }
            }

            List<DataObject> dataObjects = (List<DataObject>)CacheHandler.GetData(this.ToString(), "CustomQuery", ParameterList.ToArray());

            if (dataObjects != null)
            {
                return dataObjects;
            }

            if (this.customQueries == null)
            {
                return new List<DataObject>();
            }

            if (!(QueryIndex < this.customQueries.Count()) || ParameterNames.Count() != Parameters.Count())
            {
                return new List<DataObject>();
            }

            MySqlDataReader reader = null;

            try
            {
                MySqlCommand Command = new MySqlCommand();
                Command.CommandText = this.customQueries[QueryIndex];

                for (int i = 0; i < ParameterNames.Count(); i++)
                {
                    Command.Parameters.AddWithValue(ParameterNames[i], Parameters[i]);
                }

                reader = DatabaseHandler.ExecuteQuery(Command, this.LogDatabaseStats);

                List<DataObject> dataObjectList = this.GetDataObjects(reader);
                QueryTrace.RemoveQuery(Command.CommandText);

                CacheHandler.AddToCache(this.ToString(), "CustomQuery", ParameterList.ToArray(), dataObjectList);
                return dataObjectList;
            }
            catch (InvalidCastException)
            {
                reader.Close();
                CacheHandler.AddToCache(this.ToString(), "CustomQuery", ParameterList.ToArray(), new List<DataObject>());
                return new List<DataObject>();
            }
            catch (Exception e)
            {
                if (this.exceptionHandler(e))
                {
                    return this.CustomQuery(QueryIndex, ParameterNames, Parameters);
                }
                else
                {
                    return new List<DataObject>();
                }
            }
        }

        protected int GetCountWithCustomQuery(int QueryIndex, string[] ParameterNames, object[] Parameters)
        {
            List<string> ParameterList = new List<string>();

            try
            {
                ParameterList.Add(QueryIndex.ToString());

                foreach (object o in Parameters)
                {
                    ParameterList.Add(o.ToString());
                }
            }
            catch (Exception e)
            {
                if (!(this is IDoNotRegisterError))
                {
                    new WebsiteException(e);
                }
            }

            object Count = (object)CacheHandler.GetData(this.ToString(), "GetCountWithCustomQuery", ParameterList.ToArray());

            if (Count != null)
            {
                return (int)Count;
            }

            if (this.customQueries == null)
            {
                return 0;
            }

            MySqlDataReader reader = null;

            try
            {
                MySqlCommand Command = new MySqlCommand();
                Command.CommandText = this.customQueries[QueryIndex];

                for (int i = 0; i < ParameterNames.Count(); i++)
                {
                    Command.Parameters.AddWithValue(ParameterNames[i], Parameters[i]);
                }

                reader = DatabaseHandler.ExecuteQuery(Command, this.LogDatabaseStats);
                int CountFromDatabase = 0;

                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        CountFromDatabase = Convert.ToInt32(reader["Count"]);
                    }
                }

                DatabaseHandler.CloseConnectionByReader(reader);

                QueryTrace.RemoveQuery(Command.CommandText);
                CacheHandler.AddToCache(this.ToString(), "GetCountWithCustomQuery", ParameterList.ToArray(), CountFromDatabase);
                return CountFromDatabase;
            }
            catch (InvalidCastException)
            {
                reader.Close();
                return 0;
            }
            catch (Exception e)
            {
                if (this.exceptionHandler(e))
                {
                    return this.GetCountWithCustomQuery(QueryIndex, ParameterNames, Parameters);
                }
                else
                {
                    return 0;
                }
            }
        }

        public bool DeleteObject(DataObject dataObject)
        {
            try
            {
                MySqlCommand Command = new MySqlCommand();
                Command.CommandText = "DELETE FROM " + this.tableName + " WHERE ID = @ID";
                Command.Parameters.AddWithValue("@ID", dataObject.ID);

                DatabaseHandler.ExecuteNonQuery(Command, this.LogDatabaseStats);
                CacheHandler.ClearData(this.ToString());
                FeedbackHandler.AddMessage(dataObject.ToString() + " " + dataObject.ID + " has been deleted.");

                return true;
            }
            catch (Exception e)
            {
                if (this.exceptionHandler(e))
                {
                    return this.DeleteObject(dataObject);
                }
                else
                {
                    return false;
                }
            }
        }

        public bool DeleteObjectByID(int ID)
        {
            try
            {
                MySqlCommand Command = new MySqlCommand();
                Command.CommandText = "DELETE FROM " + this.tableName + " WHERE ID = @ID";
                Command.Parameters.AddWithValue("@ID", ID);

                DatabaseHandler.ExecuteNonQuery(Command, this.LogDatabaseStats);
                CacheHandler.ClearData(this.ToString());
                FeedbackHandler.AddMessage(this.objectType.Name.ToString() + " " + ID + " has been deleted.");

                return true;
            }
            catch (Exception e)
            {
                if (this.exceptionHandler(e))
                {
                    return this.DeleteObjectByID(ID);
                }
                else
                {
                    return false;
                }
            }
        }

        private string addLimit(string Query, int LIMIT)
        {
            return Query + " LIMIT " + LIMIT + " ";
        }

        private string addOrderBy(string Query, OrderBy orderBY, Field orderByField)
        {
            if (orderByField == null)
            {
                orderByField = this.fields[0];
            }

            return Query + " ORDER BY " + orderByField.FieldName + " " + orderBY.ToString() + " ";
        }

        private bool exceptionHandler(Exception e)
        {
            if (e.Message.EndsWith(" doesn't exist"))
            {
                try
                {
                    return this.CreateTable();
                }
                catch (Exception ex)
                {
                    if (!(this is IDoNotRegisterError))
                    {
                        new WebsiteException(ex);
                    }

                    return false;
                }
            }

            if (e.Message.StartsWith("Could not find specified column in results"))
            {
                try
                {
                    return this.RestructureTable();
                }
                catch (Exception ex)
                {
                    if (!(this is IDoNotRegisterError))
                    {
                        new WebsiteException(ex);
                    }

                    return false;
                }
            }

            if (e.Message.StartsWith("Constructor"))
            {
                throw new Exception("Constructor not foud, Please compare field types of " + this.objectType.ToString() + " and the HandlerConstructor");
            }

            if (e.Message.Contains("All Pooled Connections were in use and max pool size was reached"))
            {
                new WebsiteException(new Exception(QueryTrace.GetTrace()));
            }

            if (!(this is IDoNotRegisterError))
            {
                new WebsiteException(e);
            }

            return false;
        }

        public abstract override string ToString();
    }
}
