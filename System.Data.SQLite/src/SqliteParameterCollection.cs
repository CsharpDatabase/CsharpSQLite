//
// Community.CsharpSqlite.SQLiteClient.SqliteParameterCollection.cs
//
// Represents a collection of parameters relevant to a SqliteCommand as well as 
// their respective mappings to columns in a DataSet.
//
//Author(s):		Vladimir Vukicevic  <vladimir@pobox.com>
//			Everaldo Canuto  <everaldo_canuto@yahoo.com.br>
//			Chris Turchin <chris@turchin.net>
//			Jeroen Zwartepoorte <jeroen@xs4all.nl>
//			Thomas Zoechling <thomas.zoechling@gmx.at>
//			Alex West <alxwest@gmail.com>       
//			Stewart Adcock <stewart.adcock@medit.fr>       
//
// Copyright (C) 2002  Vladimir Vukicevic
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;
using System.Data;
using System.Data.Common;
using System.Collections;
using System.Collections.Generic ;

namespace Community.CsharpSqlite.SQLiteClient
{
	public class SqliteParameterCollection : DbParameterCollection
	{
	
		#region Fields

        List<SqliteParameter> numeric_param_list = new List<SqliteParameter>();
        Dictionary<string, int> named_param_hash = new Dictionary<string, int>();
		
		#endregion

		#region Private Methods

		private void CheckSqliteParam (object value)
		{
			if (!(value is SqliteParameter))
				throw new InvalidCastException ("Can only use SqliteParameter objects");
			SqliteParameter sqlp = value as SqliteParameter;
			if (sqlp.ParameterName == null || sqlp.ParameterName.Length == 0)
				sqlp.ParameterName = this.GenerateParameterName();
		}

		private void RecreateNamedHash ()
		{
			for (int i = 0; i < numeric_param_list.Count; i++) 
			{
				named_param_hash[((SqliteParameter) numeric_param_list[i]).ParameterName] = i;
			}
		}

		//FIXME: if the user is calling Insert at various locations with unnamed parameters, this is not going to work....
		private string GenerateParameterName()
		{
			int		index	= this.Count + 1;
			string	name	= String.Empty;

			while (index > 0)
			{
				name = ":" + index.ToString();
					if (this.IndexOf(name) == -1)
					index = -1;
				else
				index++;
			}
			return name;
		}

		#endregion

		#region Properties
		
		private bool isPrefixed (string parameterName)
		{
			return parameterName.Length > 1 && (parameterName[0] == ':' || parameterName[0] == '$' || parameterName[0] == '@');
		}

		protected override DbParameter GetParameter (int parameterIndex)
		{
			if (this.Count >= parameterIndex+1)
				return (SqliteParameter) numeric_param_list[parameterIndex];
			else          
				throw new IndexOutOfRangeException("The specified parameter index does not exist: " + parameterIndex.ToString());
		}

		protected override DbParameter GetParameter (string parameterName)
		{
			if (this.Contains(parameterName))
				return this[(int) named_param_hash[parameterName]];
			else if (isPrefixed(parameterName) && this.Contains(parameterName.Substring(1)))
				return this[(int) named_param_hash[parameterName.Substring(1)]];
			else
				throw new IndexOutOfRangeException("The specified name does not exist: " + parameterName);
		}

		protected override void SetParameter (int parameterIndex, DbParameter parameter)
		{
			if (this.Count >= parameterIndex+1)
				numeric_param_list[parameterIndex] = (SqliteParameter)parameter;
			else          
				throw new IndexOutOfRangeException("The specified parameter index does not exist: " + parameterIndex.ToString());
		}

		protected override void SetParameter (string parameterName, DbParameter parameter)
		{
			if (this.Contains(parameterName))
                numeric_param_list[(int)named_param_hash[parameterName]] = (SqliteParameter)parameter;
			else if (parameterName.Length > 1 && this.Contains(parameterName.Substring(1)))
				numeric_param_list[(int) named_param_hash[parameterName.Substring(1)]] = (SqliteParameter)parameter;
			else
				throw new IndexOutOfRangeException("The specified name does not exist: " + parameterName);
		}

		public override int Count 
		{
			get
			{
				return this.numeric_param_list.Count;
			}
		}

        public override bool IsSynchronized
        {
            get { return ((IList)this.numeric_param_list).IsSynchronized ; }
        }

        public override bool IsFixedSize
        {
            get { return ((IList)this.numeric_param_list).IsFixedSize; }
        }

        public override bool IsReadOnly
        {
            get { return ((IList)this.numeric_param_list).IsReadOnly; }
        }

        public override object SyncRoot
        {
            get { return ((IList)this.numeric_param_list).SyncRoot ; }
        }

		#endregion

		#region Public Methods

		public override void AddRange (Array values)
		{
			if (values == null || values.Length == 0)
				return;

			foreach (object value in values)
				Add (value);
		}

		public override int Add (object value)
		{
			CheckSqliteParam (value);
			SqliteParameter sqlp = value as SqliteParameter;
			if (named_param_hash.ContainsKey(sqlp.ParameterName))
				throw new DuplicateNameException ("Parameter collection already contains the a SqliteParameter with the given ParameterName.");
            numeric_param_list.Add(sqlp);
            named_param_hash.Add(sqlp.ParameterName, numeric_param_list.IndexOf(sqlp));
				return (int) named_param_hash[sqlp.ParameterName];
		}

		public SqliteParameter Add (SqliteParameter param)
		{
			Add ((object)param);
			return param;
		}
		
		public SqliteParameter Add (string name, object value)
		{
			return Add (new SqliteParameter (name, value));
		}
		
		public SqliteParameter Add (string name, DbType type)
		{
			return Add (new SqliteParameter (name, type));
		}

		public override void Clear ()
		{
			numeric_param_list.Clear ();
			named_param_hash.Clear ();
		}

		public override void CopyTo (Array array, int index)
		{
            this.numeric_param_list.CopyTo((SqliteParameter[])array, index);
		}

		public override bool Contains (object value)
		{
			return Contains ((SqliteParameter) value);
		}

		public override bool Contains (string parameterName)
		{
			return named_param_hash.ContainsKey(parameterName);
		}
		
		public bool Contains (SqliteParameter param)
		{
			return Contains (param.ParameterName);
		}

        public override IEnumerator GetEnumerator()
        {
            return this.numeric_param_list.GetEnumerator();
        }

		public override int IndexOf (object param)
		{
			return IndexOf ((SqliteParameter) param);
		}

		public override int IndexOf (string parameterName)
		{
			if (isPrefixed (parameterName)){
				string sub = parameterName.Substring (1);
				if (named_param_hash.ContainsKey(sub))
					return (int) named_param_hash [sub];
			}
			if (named_param_hash.ContainsKey(parameterName))
				return (int) named_param_hash[parameterName];
			else 
				return -1;
		}

		public int IndexOf (SqliteParameter param)
		{
			return IndexOf (param.ParameterName);
		}

		public override void Insert (int index, object value)
		{
			CheckSqliteParam (value);
			if (numeric_param_list.Count == index) 
			{
				Add (value);
				return;
			}
			
			numeric_param_list.Insert(index,(SqliteParameter) value);
			RecreateNamedHash ();
		}

		public override void Remove (object value)
		{
			CheckSqliteParam (value);
			RemoveAt ((SqliteParameter) value);
		}

		public override void RemoveAt (int index)
		{
			RemoveAt (((SqliteParameter) numeric_param_list[index]).ParameterName);
		}

		public override void RemoveAt (string parameterName)
		{
			if (!named_param_hash.ContainsKey (parameterName))
				throw new ApplicationException ("Parameter " + parameterName + " not found");
			
			numeric_param_list.RemoveAt((int) named_param_hash[parameterName]);
			named_param_hash.Remove (parameterName);
			
			RecreateNamedHash ();
		}
		
		public void RemoveAt (SqliteParameter param)
		{
			RemoveAt (param.ParameterName);
		}

		#endregion
	}
}
