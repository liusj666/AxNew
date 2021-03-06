//--------------------------------------------------------------------------------------------------//
//                                                                                                  //
//        Tie                                                                                       //
//                                                                                                  //
//          Copyright(c) Datum Connect Inc.                                                         //
//                                                                                                  //
// This source code is subject to terms and conditions of the Datum Connect Software License. A     //
// copy of the license can be found in the License.txt file at the root of this distribution. If    //
// you cannot locate the  Datum Connect Software License, please send an email to                   //
// support@datconn.com. By using this source code in any fashion, you are agreeing to be bound      //
// by the terms of the Datum Connect Software License.                                              //
//                                                                                                  //
// You must not remove this notice, or any other, from this software.                               //
//                                                                                                  //
//                                                                                                  //
//--------------------------------------------------------------------------------------------------//

using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Runtime.Serialization;

namespace AxCRL.Parser
{
    class VALL : IValizable
#if !SILVERLIGHT 
        , ISerializable
#endif
    {
        internal Type ty;   
        List<VAL> list;


        #region IValization

        public VALL(ValizationInfo info)
        {
            VAL val = info.ToVAL();
            ty = HostType.GetType((string)val[0].value);
            list = ((VALL)val[1].value).list;
        }

        public VAL GetValData()
        {
            VALL L = new VALL();
            L.Add(new VAL(ty.FullName));
            L.Add(new VAL(this));
            return new VAL(L);
        }

        #endregion

        public VALL()
        {
            list = new List<VAL>();
            ty = null;
        }

        public static VALL Clone(VALL l)
        {
            VALL L = new VALL();
            L.ty = l.ty;
            L.list = new List<VAL>();
            foreach (VAL v in l.list)
                L.list.Add(VAL.Clone(v));

            return L;
        }

        #region ISerializable

#if !SILVERLIGHT
        public VALL(SerializationInfo info, StreamingContext ctxt)
        {
            list = (List<VAL>)info.GetValue("list", typeof(List<VAL>));
            ty = (Type)info.GetValue("ty", typeof(Type));
        }

        //Serialization function.
        public void GetObjectData(SerializationInfo info, StreamingContext ctxt)
        {
            info.AddValue("list", list);
            info.AddValue("ty", ty);
        }
#endif

        #endregion

        public bool Contains(VAL v)
        {
            foreach (VAL x in list)
                if (x == v)
                    return true;
            return false;
        }

        public VALL Add(VAL v)
        {
            list.Add(v);
            return this;
        }



        #region Associative Array

        //Add Association
        public VALL Add(string key, VAL val)
        {
            VALL L = new VALL();
            L.Add(new VAL(key));
            L.Add(val);
            list.Add(new VAL(L));
            return this;
        }


        public bool IsAssociativeArray()
        {
            foreach (VAL v in list)
            {
                if (v.ty != VALTYPE.listcon)
                    return false;

                if (!((VALL)v.value).IsAssociative())
                    return false;
            }
            return true;
        }

        private bool IsAssociative()
        {
            if (list.Count != 2)
                return false;

            if (list[0].ty != VALTYPE.stringcon)
                return false;

            return true;
        }

        #endregion

        public void Clear()
        {
            list.Clear();
        }

        public bool Remove(VAL v)
        {
            return list.Remove(v);
        }


        public VALL Remove(int index)
        {
            index = Pos(index);
            list.RemoveAt(index);
            return this;
        }

        public VALL Insert(VAL v)
        {
            list.Insert(0, v);
            return this;
        }

        public VALL Insert(int index, VAL v)
        {
            index = Pos(index);
            list.Insert(index, v);
            return this;
        }

        public VALL Reverse()
        {
            list.Reverse();
            return this;
        }

        public VALL Slice(int start, int stop, int step)
        {
            VALL L = new VALL();
            for (int i = start; i <= Pos(stop); i += step)
            {
                L.Add(VAL.Clone(list[i]));
            }
            return L;
        }

        public VAL this[int pos]
        {
            get
            {
                VAL arr = new VAL(pos);
                return getter(arr, false);
            }
            set
            {
                VAL arr = new VAL(pos);
                setter(arr, value);
            }
        }

        // associative array
        // {{key1,val1},{key2,val2},....}
        public VAL this[string key]
        {
            get
            {
                VAL arr = new VAL(key);
                return getter(arr, false);
            }
            set
            {
                VAL arr = new VAL(key);
                setter(arr, value);
            }
        }


        public VAL this[VAL arr]
        {
            get
            { return getter(arr, true); }

            set
            { setter(arr, value); }
        }



        internal VAL getter(VAL arr, bool created)
        {
            switch (arr.ty)
            {

                case VALTYPE.intcon:
                    if (arr.Intcon >= 0)    //return row
                    {
                        //subscript number > size of array, increase size of array automatically
                        if (arr.Intcon >= list.Count && created)
                        {
                            while (arr.Intcon >= list.Count)
                                list.Add(new VAL());
                        }

                        if (arr.Intcon >= list.Count)
                            return VAL.VOID;

                        return list[arr.Intcon];
                    }
                    else
                    {
                        // A={2,5,1,2,7,8}
                        // A[-1] return A's last item 8
                        // A[-2] return 7
                        int pos = Pos(arr.Intcon);
                        return list[pos];
                    }


                case VALTYPE.stringcon:
                    {
                        string key = arr.Str;
                        foreach (VAL v in list)
                        {
                            if (v.ty != VALTYPE.listcon)
                                continue;

                            if (v.Size != 2 || v[0].ty != VALTYPE.stringcon)   
                                continue;

                            string prop = v[0].Str;
                            if (key == prop)
                                return v[1];
                            else if (prop == Expression.BASE_INSTANCE)  
                            {
                                VAL BS = v[1][key];
                                if (BS.Defined)
                                    return BS;
                            }
                        }

                        //not found, create new
                        if (created)
                        {
                            VAL val = new VAL();
                            this.Add(key, val);
                            return val;
                        }

                        return VAL.VOID;
                    }

                case VALTYPE.listcon:
                    {
                        VALL L = this;
                        for (int i = 0; ; i++)
                        {
                            VAL R = L.getter(arr[i], created);
                            if (i < arr.Size - 1)
                                L = R.List;
                            else
                                return R;
                        }
                    }

            }


            return VAL.VOID;
        }



        private void setter(VAL arr, VAL value)
        {
            switch (arr.ty)
            {
                case VALTYPE.intcon:
                    int pos = arr.Intcon;

                    if (pos < list.Count && pos >= 0)
                        list[pos] = value;
                    else if (pos >= 0)
                    {
                        //subscript number > size of array, increase size of array automatically
                        while (pos > list.Count)
                            list.Add(new VAL());
                        list.Add(value);
                    }
                    else    //pos < 0
                    {
                        pos = Pos(pos);
                        list[pos] = value;
                    }

                    return;


                case VALTYPE.stringcon:
                    string key = arr.Str;
                    foreach (VAL v in list)
                    {
                        if (v.ty != VALTYPE.listcon)
                            continue;

                        string prop = v[0].Str;
                        if (v.Size == 2 && v[0].ty == VALTYPE.stringcon && key == prop)
                        {
                            v.List.Remove(v[1]);
                            v.List.Add(value);
                            return;
                        }
                        else if (prop == Expression.BASE_INSTANCE) 
                        {
                            VAL BS = v[1][key];
                            if (BS.Defined)
                            {
                                HostOperation.Assign(BS, value);
                                return;
                            }
                        }

                    }

                    Add(key, value);
                    return;
            }

        }

        private int Pos(int pos)
        {
            if (pos < 0)
            {
                pos %= Size;
                if (pos < 0)
                    pos += Size;
            }

            return pos;
        }

        //set union
        public static VALL operator +(VALL L1, VALL L2)
        {
            VALL L = VALL.Clone(L1);
            foreach (VAL v in L2.list)
                L.list.Add(VAL.Clone(v));       
            return L;
        }

        //set intersection
        public static VALL operator *(VALL L1, VALL L2)
        {
            VALL L = new VALL();
            for (int i = 0; i < L1.list.Count; i++)
            {
                if (L2.list.Contains(L1.list[i]))
                    L.list.Add(L1.list[i]);
            }
            return L;
        }

        //set complmentary
        public static VALL operator -(VALL L1, VALL L2)
        {
            VALL L = VALL.Clone(L1);
            for (int i = 0; i < L1.list.Count; i++)
            {
                if (L2.list.Contains(L1.list[i]))
                    L.list.Remove(L1.list[i]);
            }
            return L;
        }

        public static bool operator !=(VALL L1, VALL L2)
        {
            return !(L1 == L2);
        }

        public static bool operator ==(VALL L1, VALL L2)
        {
            if (L1.list.Count != L2.list.Count)
                return false;

            for (int i = 0; i < L1.list.Count; i++)
                if (L1[i] != L2[i])
                    return false;

            return true;
        }

        public static bool operator >(VALL L1, VALL L2)
        {
            if (L1.list.Count != L2.list.Count)
                return false;

            for (int i = 0; i < L1.list.Count; i++)
                if (!(L1[i] > L2[i]))
                    return false;

            return true;

        }
        public static bool operator <(VALL L1, VALL L2)
        {
            return L2 > L1 && L1 != L2;
        }

        public override bool Equals(Object o)
        {
            return this == (VALL)o;
        }

        public override int GetHashCode()
        {
            return 0;
        }

        public int Size
        {
            get
            {
                return list.Count;
            }

        }
        public override String ToString()
        {
            return encode(true, true, false);
        }

        public String ToString2()
        {
            return encode(false, true, false);
        }

        internal String encode(bool quotationMark, bool nullMark, bool persistent)
        {

            StringWriter o = new StringWriter();
            if (quotationMark) o.Write("{");
            if (list.Count > 0)
            {
                for (int i = 0; i < list.Count - 1; i++)
                    o.Write("{0},", list[i].encode(quotationMark, nullMark, persistent));

                o.Write("{0}", list[list.Count - 1].encode(quotationMark, nullMark, persistent));
            }
            if (quotationMark) o.Write("}");

            return o.ToString();

        }

        public object[] ObjectArray
        {
            get
            {
                if (list.Count == 0)
                    return new object[] { };

                object[] values = new object[list.Count];

                int i = 0;
                foreach (VAL val in list)
                {
                    values[i++] = val.HostValue;
                }

                return values;
            }
        }

        public object HostValue
        {
            get
            {
                Array array;

                if (ty != null)
                {
                    if (ty.IsArray)
                    {
                        int rank = ty.GetArrayRank();
                        if (rank == 1)
                            array = (Array)Activator.CreateInstance(ty, new object[] { list.Count });
                        else 
                        {
                            object[] len = new object[rank]; 
                            int k = 0;
                            VALL L = this;
                            for (; ; )
                            {
                                len[k++] = L.Size;
                                if (L[0].Size > 0 && k < rank)
                                    L = L[0].List;  
                                else
                                {
                                    if (k < rank)
                                        throw new TieException("Faild to cast to multidimensional array: VAL dimension < Array rank.");

                                    break;
                                }
                            }

                            array = (Array)Activator.CreateInstance(ty, len);
                            VALL.SetValue(array, new VAL(this));
                            return array;
                        }
                    }
                    else
                    {
                        object host;
                        if (HostCoding.HasContructor(ty, new Type[] { typeof(VAL) }))
                        {
                            host = Activator.CreateInstance(ty, new object[] { new VAL(this) });  
                        }
                        else
                        {
                            host = Activator.CreateInstance(ty, new object[] { });    
                            HostValization.Val2Host(new VAL(this), host);
                        }

                        return host;
                    }
                }
                else
                    array = new object[list.Count];

                int i = 0;
                foreach (VAL val in list)
                {
                    array.SetValue(val.HostValue, i++);
                }

                if (ty == null)
                    return HostCoding.ToHostArray((object[])array);
                else
                    return array;
            }


        }

        private static int Sigma(Array I, int d)
        {
            int sigma = 1;
            for (int i = d; i < I.Rank; i++)
                sigma *= I.GetLength(i);

            return sigma;
        }

        private static void SetValue(Array A, VAL V)
        {
            int[] indices = new int[A.Rank];

            int i = 0;
            int L = Sigma(A, 0);    
            while (i < L)
            {
                for (int k = 0; k < A.Rank - 1; k++)
                    indices[k] = (i / Sigma(A, k + 1)) % A.GetLength(k);  

                indices[A.Rank - 1] = i % A.GetLength(A.Rank - 1);        

                i++;
                A.SetValue(V[new VAL(indices)].HostValue, indices);
            }

        }
    }
}
