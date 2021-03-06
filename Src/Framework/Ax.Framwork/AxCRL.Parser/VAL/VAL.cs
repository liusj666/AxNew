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
using System.Collections;
using System.Runtime.Serialization;
using System.Text;
using System.IO;
using System.Reflection;

namespace AxCRL.Parser
{
    /// <summary>
    /// represent VAL type
    /// </summary>
    public enum VALTYPE
    {
        /// <summary>
        /// value: void 
        /// </summary>
        voidcon = 0,

        /// <summary>
        /// value: null
        /// </summary>
        nullcon = 1,

        /// <summary>
        /// boolean: true or false
        /// </summary>
        boolcon = 2,

        /// <summary>
        /// value: integer, short, byte
        /// </summary>
        intcon = 3,

        /// <summary>
        /// value: double, float, single
        /// </summary>
        doublecon = 4,

        /// <summary>
        /// value: string, support UNICODE
        /// </summary>
        stringcon = 5,
        
        /// <summary>
        /// value: decimal, long
        /// </summary>
        decimalcon = 6,

        /// <summary>
        /// value: list, associative array
        /// </summary>
        listcon = 7,

        /// <summary>
        /// value: function
        /// </summary>
        funccon = 8,

        /// <summary>
        /// value: class
        /// </summary>
        classcon = 9,       


        /// <summary>
        /// value: .net object
        /// </summary>
        hostcon = 20,

        /// <summary>
        /// value: script code
        /// </summary>
        scriptcon = 21,

        /// <summary>
        /// value: address in the memory
        /// </summary>
        addrcon = 30,

        /// <summary>
        /// value: offset of structure
        /// </summary>
        identcon = 31,

    }
    
    enum HandlerActionType
    {
        None,
        Add,
        Remove
    }

    /// <summary>
    /// Any data in the Tie is VAL
    /// </summary>
    public class VAL : IValizable, ICollection<VAL>, IEnumerable<VAL>, IEnumerable, IComparable, IComparable<VAL>, IEquatable<VAL>
#if !SILVERLIGHT
        ,ISerializable
        ,ICloneable
#endif
    {
        /// <summary>
        /// internal value of VAL
        /// </summary>
        public object value;
        /// <summary>
        /// type of value
        /// </summary>
        public VALTYPE ty;

        internal string Class;
        internal string name;
        internal HandlerActionType hty = HandlerActionType.None;    //indicate Add/Remove event

        internal object temp;  


        #region IValization
        /// <summary>
        /// Valization 
        /// </summary>
        /// <param name="info"></param>
        public VAL(ValizationInfo info)
        {
            VAL val = info.ToVAL(); 
               ty = (VALTYPE)val[0].value;
            value = val[1].HostValue;
            hty = (HandlerActionType)val[2].value;
            Class = (string)val[3].value;
        }

        /// <summary>
        /// IValizable function.
        /// </summary>
        /// <returns></returns>
        public VAL GetValData()
        {
            VAL val = new VAL(new object[]{(int)ty,value,(int)hty, Class});
            val.Class = null;

            return val;
        }
        #endregion



        #region ICloneable/IComparable/IComparable<VAL>/IEquatable<VAL>/ISerializable
        /// <summary>
        /// reates a new object that is a copy of the current instance.
        /// </summary>
        /// <returns> A new object that is a copy of this instance.</returns>
        public object Clone()
        {
            return VAL.Clone(this);
        }

        /// <summary>
        /// Compares the current instance with another object of the same type and returns
        ///     an integer that indicates whether the current instance precedes, follows,
        ///     or occurs in the same position in the sort order as the other object.
        /// </summary>
        /// <param name="v">An object to compare with this instance.</param>
        /// <returns>A 32-bit signed integer that indicates the relative order of the objects
        ///     being compared. The return value has these meanings: Value Meaning Less than
        ///     zero This instance is less than obj. Zero This instance is equal to obj.
        ///     Greater than zero This instance is greater than obj.
        ///</returns>
        public int CompareTo(VAL v)
        {
            if (this == v)
                return 0;
            else if (this > v)
                return 1;
            else
                return -1;
        }

        int IComparable.CompareTo(object v)
        {
            if(v is VAL)
                return CompareTo((VAL)v);
            
            throw new InvalidCastException("cannot compare non VAL"); 
        }

        /// <summary>
        ///  Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="o"> An object to compare with this object.</param>
        /// <returns>true if the current object is equal to the other parameter; otherwise, false.</returns>
        public bool Equals(VAL o)
        {
            return this == o;
        }

#if !SILVERLIGHT
        /// <summary>
        /// Create instance from SerializationInfo
        /// </summary>
        /// <param name="info"></param>
        /// <param name="ctxt"></param>
        public VAL(SerializationInfo info, StreamingContext ctxt)
        {
            ty = (VALTYPE)info.GetValue("ty", typeof(VALTYPE));
            Class = (string)info.GetValue("Class", typeof(string));
            value = info.GetValue("value", typeof(object));
        }

       
        /// <summary>
        ///  Serialization function.
        /// </summary>
        /// <param name="info"></param>
        /// <param name="ctxt"></param>
        public void GetObjectData(SerializationInfo info, StreamingContext ctxt)
        {
            info.AddValue("ty", ty);
            info.AddValue("Class", Class);
            info.AddValue("value", value);
        }
#endif

        #endregion

        #region VAL Contructor
        
        /// <summary>
        /// create instance with value = null
        /// </summary>
        public VAL()
        {
            ty = VALTYPE.nullcon;
            value = null;
        }
 
        /// <summary>
        /// create instance with value = boolean
        /// </summary>
        /// <param name="b"></param>
        public VAL(bool b)
        {
            ty = VALTYPE.boolcon;
            value = b;
        }

        /// <summary>
        /// create instance with value = integer
        /// </summary>
        /// <param name="i"></param>
        public VAL(int i)
        {
            ty = VALTYPE.intcon;
            value = i;
        }

        /// <summary>
        /// create instance with value = double
        /// </summary>
        /// <param name="d"></param>
        public VAL(double d)
        {
            ty = VALTYPE.doublecon;
            value = d;
        }

        /// <summary>
        /// create instance with value = decimal
        /// </summary>
        /// <param name="d"></param>
        public VAL(decimal d)
        {
            ty = VALTYPE.decimalcon;
            value = d;
        }

        /// <summary>
        /// create instance with value = string
        /// </summary>
        /// <param name="str"></param>
        public VAL(string str)
        {
            ty = VALTYPE.stringcon;
            value = str;
        }


        //list
        internal VAL(VALL list)
        {
            ty = VALTYPE.listcon;
            value = list;
        }

        internal VAL(Operand opr)
        {
            switch (opr.ty)
            { 
      
                case OPRTYPE.numcon:
                    Numeric c = (Numeric)(opr.value);
                    switch (c.ty)
                    { 
                        case NUMTYPE.boolcon:
                            ty = VALTYPE.boolcon;
                            break;

                        case NUMTYPE.doublecon:
                            ty = VALTYPE.doublecon;
                            break;

                        case NUMTYPE.intcon:
                            ty = VALTYPE.intcon;
                            break;

                        case NUMTYPE.stringcon:
                            ty = VALTYPE.stringcon;
                            break;

                        case NUMTYPE.nullcon:
                            ty = VALTYPE.nullcon;
                            break;

                        case NUMTYPE.voidcon:
                            ty = VALTYPE.voidcon;
                            break;
                    }
                    value = c.value;
                    break;

                case OPRTYPE.classcon:
                    ty = VALTYPE.classcon;
                    value = opr.value;
                    break;
                case OPRTYPE.funccon:
                    ty = VALTYPE.funccon;
                    value = opr.value;
                    break;

                case OPRTYPE.addrcon:
                    ty = VALTYPE.addrcon;
                    value = opr.value;
                    break;

                case OPRTYPE.identcon:
                    ty = VALTYPE.identcon;
                    value = opr.value;
                    break;

                //case OperandType.intcon:
                //case OperandType.none:
                //case OperandType.regcon:
                default :
                    throw new TieException("{0} not supported in VAL", opr.ty);

            }


            name = opr.name;
            Class = opr.mod;
        }


        /// <summary>
        /// create instance with value = .NET object
        /// </summary>
        /// <param name="host"></param>
        /// <returns></returns>
        public static VAL NewHostType(object host)
        {
            VAL v = new VAL();
            v.ty = VALTYPE.hostcon;

            v.value = host;

            if (host != null)
                v.Class = v.value.GetType().UnderlyingSystemType.FullName;
            else
                v.Class = "";

            return v;
        }

        /// <summary>
        /// create instance with value = varible dictionary
        /// </summary>
        /// <param name="memory"></param>
        public VAL(Memory memory)
        {
            VAL v = Memory.Assemble(memory);
            this.ty = v.ty;
            this.Class = v.Class;
            this.value = v.value;
        }

        /// <summary>
        /// create instance with value = Jagged Array
        /// </summary>
        /// <param name="A"></param>
        public VAL(Array A)
        {
            VALL L = new VALL();
            L.ty = A.GetType();
            foreach (object obj in A)
                L.Add(VAL.Boxing(obj));

            this.ty = VALTYPE.listcon;
            this.Class = L.ty.FullName;
            this.value = L;
        }

        #endregion

        internal static VAL VOID
        {
            get
            {
                VAL v = new VAL();
                v.ty = VALTYPE.voidcon;

                return v;
            }
        }

        internal static VAL Clone(VAL v)
        {
            VAL V = new VAL();
            V.ty = v.ty;
            if (v.ty != VALTYPE.listcon)
                V.value = v.value;
            else
            {
                V.value = AxCRL.Parser.VALL.Clone(v.List);
            }
            V.Class = v.Class;
            V.name = v.name;
            V.hty = v.hty;
            V.temp = v.temp;
            return V;
        }


        internal static VAL Script(string script)
        {
            VAL v = new VAL();
            v.ty = VALTYPE.scriptcon;
            v.value = script;
            return v;
        }


        #region Boxing/UnBox
        
        /// <summary>
        /// Box any object into VAL, support multiple dimension array
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        public static VAL Boxing(object v)
        {
            if (v is Array)
            {
                VAL L = VAL.Array();
                Array A = (Array)v;
                foreach (object obj in A)
                    L.List.Add(VAL.Boxing(obj));

                int[] lengths = new int[A.Rank];
                for (int i = 0; i < lengths.Length; i++)
                    lengths[i] = A.GetLength(i);

                L = multiDimensionalArray(L, lengths, A.Rank - 1);
                L.List.ty = A.GetType();
                L.Class = L.List.ty.FullName; 
                return L;
            }
            return Boxing1(v);
        }

     

        
        /// <summary>
        /// Box any object into VAL without support array
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        internal static VAL Boxing1(object v)
        {
            if (v == null)
                return new VAL();
            else if (v == System.DBNull.Value)
            {
                VAL x = new VAL();
                x.Type = typeof(DBNull);
                return x;
            }

            else if (v is string)
                return new VAL((string)v);
            else if (v is bool)
                return new VAL((bool)v);
            else if (v is int)
                return new VAL((int)v);
            else if (v is double)
                return new VAL((double)v);
            else if (v is decimal)
                return new VAL((decimal)v);

            else if (v is byte)
                return new VAL(Convert.ToInt32(v));
            else if (v is sbyte)
                return new VAL(Convert.ToInt32(v));
            else if (v is short)
                return new VAL(Convert.ToInt32(v));
            else if (v is ushort)
                return new VAL(Convert.ToInt32(v));
            else if (v is uint)
                return new VAL(Convert.ToDecimal(v));
            else if (v is long)
                return new VAL(Convert.ToDecimal(v));
            else if (v is ulong)
                return new VAL(Convert.ToDecimal(v));

            else if (v is float)
                return new VAL(Convert.ToDouble(v));
            else if (v is char)
                return new VAL(((char)v).ToString());
            

            else if (v is VAL)
                return (VAL)v;
            else if (v is VALL)
                return new VAL((VALL)v);
            else if (v is Memory)
                return new VAL((Memory)v);

            
            return VAL.NewHostType(v);
        }

        private static VAL multiDimensionalArray(VAL A, int[] lengths, int d)
        {
            if (d == 0)
                return A;

            VAL L1 = VAL.Array();
            int len = lengths[d];
            
            VAL L2 = VAL.Array();
            for (int i = 0; i < A.Size; i++)
            {
                L2.Add(A[i]);
                if ((i + 1) % len == 0)
                {
                    L1.Add(L2);
                    L2 = VAL.Array();
                }
            }

            return multiDimensionalArray(L1, lengths, d - 1);
        }

        /// <summary>
        /// Unbox VAL to .NET host object
        /// </summary>
        /// <param name="val"></param>
        /// <returns></returns>
        public static object UnBoxing(VAL val)
        { 
            return val.HostValue;
        }

      

   
        #endregion


        #region VAL Operator v=-exp; v=exp1+exp2; v=exp1-exp2;

        /// <summary>
        /// not operator, support operator overloading in host object
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        public static bool operator !(VAL v)
        {
            switch (v.ty)
            {
                case VALTYPE.boolcon:
                    return !(bool)(v.value);

                case VALTYPE.hostcon:
                    VAL v1 = HostFunction.OperatorOverloading(Operator.op_LogicalNot, v);
                    if(v1.ty == VALTYPE.boolcon)
                        return (bool)v1.value;
                    
                    throw new InvalidOperationException("overloading operator !(.) must return bool value."); 

            }

            return false;
        }

        /// <summary>
        /// negative operator, support operator overloading in host object
        /// </summary>
        /// <param name="v1"></param>
        /// <returns>return reversed list if v1 is list</returns>
        public static VAL operator -(VAL v1)
        {
            VAL v = VAL.Clone(v1);

            switch (v.ty)
            {
                case VALTYPE.intcon:
                    v.value = -(int)(v.value);
                    return v;

                case VALTYPE.doublecon:
                    v.value = -(double)(v.value);
                    return v;

                case VALTYPE.decimalcon:
                    v.value = -(decimal)(v.value);
                    return v;

                case VALTYPE.listcon:
                    v.List.Reverse();
                    return v;

                case VALTYPE.stringcon:
                    break;

                case VALTYPE.hostcon:
                    return HostFunction.OperatorOverloading(Operator.op_UnaryNegation, v);

            }

            return v;
        }

        /// <summary>
        ///  positive operator, support operator overloading in host object
        /// </summary>
        /// <param name="v1"></param>
        /// <returns></returns>
        public static VAL operator +(VAL v1)
        {
            VAL v = VAL.Clone(v1);

            switch (v.ty)
            {
                case VALTYPE.intcon:
                case VALTYPE.doublecon:
                case VALTYPE.decimalcon:
                    return v;

                case VALTYPE.listcon:
                    return v;

                case VALTYPE.hostcon:
                    return HostFunction.OperatorOverloading(Operator.op_UnaryPlus, v);

            }

            return v;
        }

        /// <summary>
        /// + operator, support operator overloading in host object
        ///     add eventhandler
        ///     concatenate 2 lists
        /// </summary>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        /// <returns></returns>
        public static VAL operator +(VAL v1, VAL v2)
        {

            if (v1.ty == VALTYPE.hostcon)
            {
                //Falut-Tolerance Design
                if (v2.IsAssociativeArray())
                {
                    HostValization.Val2Host(v2, v1.value);
                    return v1;
                }

                if (v1.value is EventInfo)
                {
                    if (v2.ty == VALTYPE.funccon)       //event handler
                    {
                        HostEvent hostEvent = new HostEvent(v1.value as EventInfo, v2);
                        return hostEvent.AddDelegateEventHandler();
                    }
                    else
                        throw new HostTypeException("{0} is not event handler of {1}", v2, v1.value);
                }

            }


            VAL v = VAL.Clone(v1);

            if (v1.ty == VALTYPE.listcon)
            {
                if (v2.ty == VALTYPE.listcon)
                    v.value = v1.List + v2.List;
            }

            else if (v2.ty == VALTYPE.listcon)
            {
                VAL t = VAL.Clone(v2);
                t.List.Insert(v);
                return t;
            }

            else if (v1.ty == VALTYPE.stringcon || v2.ty == VALTYPE.stringcon)
            {
                v.ty = VALTYPE.stringcon;
                v.value = v1.ToString2() + v2.ToString2();
            }
            else if (v1.ty == VALTYPE.hostcon)
            {
                return HostFunction.OperatorOverloading(Operator.op_Addition, v1, v2);
            }
            else
                switch (v1.ty)
                {
                    case VALTYPE.nullcon:
                        switch (v2.ty)
                        {
                            case VALTYPE.intcon:
                            case VALTYPE.doublecon:
                            case VALTYPE.decimalcon:
                                v.ty = v2.ty;
                                v.value = v2.value; 
                                break;

                        }
                        break;


                    case VALTYPE.intcon:
                        switch (v2.ty)
                        {
                            case VALTYPE.intcon: v.value = (int)(v1.value) + (int)(v2.value); break;
                            case VALTYPE.doublecon: v.ty = v2.ty; v.value = (int)(v1.value) + (double)(v2.value); break;
                            case VALTYPE.decimalcon: v.ty = v2.ty; v.value = (int)(v1.value) + (decimal)(v2.value); break;
                        }
                        break;

                    case VALTYPE.doublecon:
                        switch (v2.ty)
                        {
                            case VALTYPE.intcon: v.value = (double)(v1.value) + (int)(v2.value); break;
                            case VALTYPE.doublecon: v.value = (double)(v1.value) + (double)(v2.value); break;
                            case VALTYPE.decimalcon: v.value = (double)(v1.value) + (double)((decimal)(v2.value)); break;
                        }
                        break;


                    case VALTYPE.decimalcon:
                        switch (v2.ty)
                        {
                            case VALTYPE.intcon: v.value = (decimal)(v1.value) + (int)(v2.value); break;
                            case VALTYPE.doublecon: v.ty = v2.ty;  v.value = (double)((decimal)(v1.value)) + (double)(v2.value); break;
                            case VALTYPE.decimalcon: v.value = (decimal)(v1.value) +(decimal)(v2.value); break;
                        }
                        break;

                    default:
                        return new VAL();
                }

            return v;
        }

        /// <summary>
        ///  - operator, support operator overloading in host object
        ///     remove v2 elements from v1 if v1 is list
        /// </summary>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        /// <returns></returns>
        public static VAL operator -(VAL v1, VAL v2)
        {

            if (v1.ty == VALTYPE.hostcon && v1.value is EventInfo)
            {
                if (v2.ty == VALTYPE.funccon)       //event handler
                {
                    HostEvent hostEvent = new HostEvent(v1.value as EventInfo, v2);
                    return hostEvent.RemoveDelegateEventHandler();
                }
                else
                    throw new HostTypeException("{0} is not event handler of {1}", v2, v1.value);
            }


            VAL v = VAL.Clone(v1);

            if (v1.ty == VALTYPE.listcon)
            {
                if (v2.ty == VALTYPE.listcon)
                    v.value = v1.List - v2.List;
                else
                    v.List.Remove(v2);
            }

            else if (v2.ty == VALTYPE.listcon)
            {
                v = VAL.Clone(v2);
                v.List.Insert(v1);
            }

            else if (v1.ty == VALTYPE.stringcon || v2.ty == VALTYPE.stringcon)
            {
                v1.ty = VALTYPE.stringcon;
                v1.value = v1.Str + v2.Str;
            }
            else if (v1.ty == VALTYPE.hostcon)
            {
                return HostFunction.OperatorOverloading(Operator.op_Subtraction, v1, v2);
            }
            else
                switch (v1.ty)
                {
                    case VALTYPE.nullcon:
                        switch (v2.ty)
                        {
                            case VALTYPE.intcon:
                                v.ty = v2.ty;
                                v.value = -(int)v2.value; break;
                            case VALTYPE.doublecon:
                                v.ty = v2.ty;
                                v.value = -(double)v2.value; break;
                            case VALTYPE.decimalcon:
                                v.ty = v2.ty;
                                v.value = -(decimal)v2.value; break;

                        }
                        break;

                    case VALTYPE.intcon:
                        switch (v2.ty)
                        {
                            case VALTYPE.intcon: v.value = (int)(v1.value) - (int)(v2.value); break;
                            case VALTYPE.doublecon: v.ty = v2.ty; v.value = (int)(v1.value) - (double)(v2.value); break;
                            case VALTYPE.decimalcon: v.ty = v2.ty; v.value = (int)(v1.value) - (decimal)(v2.value); break;
                        }
                        break;

                    case VALTYPE.doublecon:
                        switch (v2.ty)
                        {
                            case VALTYPE.intcon: v.value = (double)(v1.value) - (int)(v2.value); break;
                            case VALTYPE.doublecon: v.value = (double)(v1.value) - (double)(v2.value); break;
                            case VALTYPE.decimalcon: v.value = (double)(v1.value) - (double)((decimal)(v2.value)); break;
                        }
                        break;

                    case VALTYPE.decimalcon:
                        switch (v2.ty)
                        {
                            case VALTYPE.intcon: v.value = (decimal)(v1.value) - (int)(v2.value); break;
                            case VALTYPE.doublecon: v.ty = v2.ty;  v.value = (double)((decimal)(v1.value)) - (double)(v2.value); break;
                            case VALTYPE.decimalcon: v.value = (decimal)(v1.value) - (decimal)(v2.value); break;
                        }
                        break;
                    default:
                        return new VAL();
                }

            return v;
        }


        #endregion


        #region Operator v=exp1*exp2; v=exp1/exp2; v=exp1%exp2;

        /// <summary>
        /// * operator, support operator overloading in host object
        /// </summary>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        /// <returns></returns>
        public static VAL operator *(VAL v1, VAL v2)
        {

            VAL v = VAL.Clone(v1);

            switch (v1.ty)
            {
                case VALTYPE.intcon:
                    switch (v2.ty)
                    {
                        case VALTYPE.intcon: v.value = (int)(v1.value) * (int)(v2.value); break;
                        case VALTYPE.doublecon: v.ty = v2.ty; v.value = (int)(v1.value) * (double)(v2.value); break;
                        case VALTYPE.decimalcon: v.ty = v2.ty; v.value = (int)(v1.value) * (decimal)(v2.value); break;
                    }
                    break;

                case VALTYPE.doublecon:
                    switch (v2.ty)
                    {
                        case VALTYPE.intcon: v.value = (double)(v1.value) * (int)(v2.value); break;
                        case VALTYPE.doublecon: v.value = (double)(v1.value) * (double)(v2.value); break;
                        case VALTYPE.decimalcon: v.value = (double)(v1.value) * (double)((decimal)(v2.value)); break;
                    }
                    break;

                case VALTYPE.decimalcon:
                    switch (v2.ty)
                    {
                        case VALTYPE.intcon: v.value = (decimal)(v1.value) * (int)(v2.value); break;
                        case VALTYPE.doublecon: v.ty = v2.ty; v.value = (double)((decimal)(v1.value)) * (double)(v2.value); break;
                        case VALTYPE.decimalcon: v.value = (decimal)(v1.value) * (decimal)(v2.value); break;
                    }
                    break;

                case VALTYPE.listcon:
                    switch (v2.ty)
                    {
                        case VALTYPE.listcon: v.value = v1.List * v2.List; break;
                    }
                    break;

                case VALTYPE.stringcon:
                    switch (v2.ty)
                    {
                        case VALTYPE.intcon: for (int i = 1; i < v2.Intcon; i++) v.Str += v1.Str; break;
                    }
                    break;

                case VALTYPE.hostcon:
                    return HostFunction.OperatorOverloading(Operator.op_Multiply, v1, v2);

                default:
                    return new VAL();
            }

            return v;
        }

        /// <summary>
        /// / operator, support operator overloading in host object
        /// </summary>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        /// <returns></returns>
        public static VAL operator /(VAL v1, VAL v2)
        {

            VAL v = VAL.Clone(v1);

            switch (v1.ty)
            {
                case VALTYPE.intcon:
                    switch (v2.ty)
                    {
                        case VALTYPE.intcon: v.value = (int)(v1.value) / (int)(v2.value); break;
                        case VALTYPE.doublecon: v.ty = v2.ty; v.value = (int)(v1.value) / (double)(v2.value); break;
                        case VALTYPE.decimalcon: v.ty = v2.ty; v.value = (int)(v1.value) / (decimal)(v2.value); break;
                    }
                    break;

                case VALTYPE.doublecon:
                    switch (v2.ty)
                    {
                        case VALTYPE.intcon: v.value = (double)(v1.value) / (int)(v2.value); break;
                        case VALTYPE.doublecon: v.value = (double)(v1.value) / (double)(v2.value); break;
                        case VALTYPE.decimalcon: v.value = (double)(v1.value) / (double)((decimal)(v2.value)); break;
                    }
                    break;

                case VALTYPE.decimalcon:
                    switch (v2.ty)
                    {
                        case VALTYPE.intcon: v.value = (decimal)(v1.value) / (int)(v2.value); break;
                        case VALTYPE.doublecon: v.ty = v2.ty; v.value = (double)((decimal)(v1.value)) / (double)(v2.value); break;
                        case VALTYPE.decimalcon: v.value = (decimal)(v1.value) / (decimal)(v2.value); break;
                    }
                    break;

                case VALTYPE.hostcon:
                    return HostFunction.OperatorOverloading(Operator.op_Division, v1, v2);

                default:
                    return new VAL();
            }

            return v;
        }

        /// <summary>
        /// % operator, support operator overloading in host object
        /// </summary>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        /// <returns></returns>
        public static VAL operator %(VAL v1, VAL v2)
        {
            VAL v = VAL.Clone(v1);
            switch (v1.ty)
            {
                case VALTYPE.intcon:
                    switch (v2.ty)
                    {
                        case VALTYPE.intcon: v.value = (int)(v1.value) % (int)(v2.value); break;
                        case VALTYPE.doublecon: v.ty = v2.ty; v.value = (int)(v1.value) % (double)(v2.value); break;
                        case VALTYPE.decimalcon: v.ty = v2.ty; v.value = (int)(v1.value) % (decimal)(v2.value); break;
                    }
                    break;

                case VALTYPE.doublecon:
                    switch (v2.ty)
                    {
                        case VALTYPE.intcon: v.value = (double)(v1.value) % (int)(v2.value); break;
                        case VALTYPE.doublecon: v.value = (double)(v1.value) % (double)(v2.value); break;
                        case VALTYPE.decimalcon: v.value = (double)(v1.value) % (double)((decimal)(v2.value)); break;
                    }
                    break;

                case VALTYPE.decimalcon:
                    switch (v2.ty)
                    {
                        case VALTYPE.intcon: v.value = (decimal)(v1.value) % (int)(v2.value); break;
                        case VALTYPE.doublecon: v.ty = v2.ty; v.value = (double)((decimal)(v1.value)) % (double)(v2.value); break;
                        case VALTYPE.decimalcon: v.value = (decimal)(v1.value) % (decimal)(v2.value); break;
                    }
                    break;

                case VALTYPE.hostcon:
                    return HostFunction.OperatorOverloading(Operator.op_Modulus, v1, v2);

                default:
                    return new VAL();
            }

            return v;
        }

        #endregion


        #region Operator  ==, >, <, <=, >= !=

        /// <summary>
        /// == operator, support operator overloading in host object
        /// </summary>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        /// <returns></returns>
        public static bool operator ==(VAL v1, VAL v2)
        {
            if (v1.isNumber && v2.isNumber)
                return CompareNumber(v1, v2) == 0;

            if (v1.ty != v2.ty)
                return false;

            switch (v1.ty)
            {
                case VALTYPE.voidcon: return v2.ty == VALTYPE.voidcon;
                case VALTYPE.nullcon: return v2.ty == VALTYPE.nullcon;
                case VALTYPE.boolcon: return (bool)(v1.value) == (bool)(v2.value);
                case VALTYPE.listcon: return (VALL)(v1.value) == (VALL)(v2.value);

//                case VALTYPE.addrcon: return (int)(v1.value) == (int)(v2.value);
                case VALTYPE.stringcon: return (string)(v1.value) == (string)(v2.value);

                case VALTYPE.classcon:
                case VALTYPE.funccon:
                    return (int)(v1.value) == (int)(v2.value)
                        && v1.Class == v2.Class;

                case VALTYPE.hostcon:
                    return HostOperation.HostCompareTo(Operator.op_Equality, v1, v2) == 0;

            }
            return false;
        }

        /// <summary>
        /// > operator, support operator overloading in host object
        /// </summary>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        /// <returns></returns>
        public static bool operator >(VAL v1, VAL v2)
        {
            if (v1.isNumber && v2.isNumber)
                return CompareNumber(v1, v2) > 0;
         

            if (v1.ty == v2.ty)
            {
                switch (v1.ty)
                {
                    case VALTYPE.boolcon: return (int)(v1.value) > (int)(v2.value);

                    //case VALTYPE.identcon:
                    case VALTYPE.stringcon:
                        return 0 > string.Compare((string)v1.value, (string)v2.value);
                    case VALTYPE.listcon:
                        return (VALL)(v1.value) > (VALL)(v2.value);
                    case VALTYPE.hostcon:
                        return HostOperation.HostCompareTo(Operator.op_GreaterThan, v1, v2) > 0;
                }
            }
            else if (v1.ty == VALTYPE.listcon)
            {
                if (v1.List.Contains(v2))
                    return true;

            } 

            return false;

        }


        /// <summary>
        /// Less than operator, support operator overloading in host object
        /// </summary>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        /// <returns></returns>
        public static bool operator <(VAL v1, VAL v2)
        {
            if (v1.isNumber && v2.isNumber)
                return CompareNumber(v1, v2) < 0;
         
            if (v1.ty == v2.ty)
            {
                switch (v1.ty)
                {
                    case VALTYPE.boolcon: return (int)(v1.value) < (int)(v2.value);

                    //case VALTYPE.identcon:
                    case VALTYPE.stringcon:
                    case VALTYPE.funccon:
                        return 0 < String.Compare((String)v1.value, (String)v2.value);
                    case VALTYPE.listcon:
                        return (VALL)(v1.value) < (VALL)(v2.value);
                    case VALTYPE.hostcon:
                        return HostOperation.HostCompareTo(Operator.op_LessThan, v1, v2) < 0;
      
                }
            }
            else if (v2.ty == VALTYPE.listcon)
            {
                if (v2.List.Contains(v1))
                    return true;

            }
            return false;

        }


        /// <summary>
        /// >= operator, support operator overloading in host object
        /// </summary>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        /// <returns></returns>
        public static bool operator >=(VAL v1, VAL v2)
        {
            return v1 == v2 || v1 > v2;
        }

        /// <summary>
        /// less and equal than operator, support operator overloading in host object
        /// </summary>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        /// <returns></returns>
        public static bool operator <=(VAL v1, VAL v2)
        {
            return v1 == v2 || v1 < v2;
        }

        /// <summary>
        /// != operator, support operator overloading in host object
        /// </summary>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        /// <returns></returns>
        public static bool operator !=(VAL v1, VAL v2)
        {
            return !(v1 == v2);
        }

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="o"></param>
        /// <returns></returns>
        public override bool Equals(Object o)
        {
            return this == (VAL)o;
        }

        /// <summary>
        /// Returns hash code
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return 0;
        }

        private bool isNumber
        {
            get
            {
                return ty == VALTYPE.intcon || ty == VALTYPE.doublecon || ty == VALTYPE.decimalcon;
            }
        }

        private static int CompareNumber(VAL v1, VAL v2)
        {
            int d=0;
            switch (v1.ty)
            {
                case VALTYPE.intcon:
                    switch (v2.ty)
                    {
                        case VALTYPE.intcon: d = (int)(v1.value) - (int)(v2.value); break;
                        case VALTYPE.doublecon: d = (int)((int)(v1.value) - (double)(v2.value)); break;
                        case VALTYPE.decimalcon: d = (int)((int)(v1.value) - (decimal)(v2.value)); break;
                    }
                    break;
                case VALTYPE.doublecon:
                    switch (v2.ty)
                    {
                        case VALTYPE.intcon: d = (int)((double)(v1.value) - (int)(v2.value)); break;
                        case VALTYPE.doublecon: d = (int)((double)(v1.value) - (double)(v2.value)); break;
                        case VALTYPE.decimalcon: d = (int)((double)(v1.value) - (double)((decimal)(v2.value))); break;
                    }
                    break;
                case VALTYPE.decimalcon:
                    switch (v2.ty)
                    {
                        case VALTYPE.intcon: d = (int)((decimal)(v1.value) - (int)(v2.value)); break;
                        case VALTYPE.doublecon: d = (int)((double)((decimal)(v1.value)) - (double)(v2.value)); break;
                        case VALTYPE.decimalcon: d = (int)((decimal)(v1.value) - (decimal)(v2.value)); break;
                    }
                    break;
            }

            if (d < 0)
                return -1;
            else if (d > 0)
                return 1;
            else
                return 0;
        }

        #endregion

        #region Operator v = ~exp, v=exp1&exp2, v=exp1|exp2, v=exp1^exp2

        /// <summary>
        /// ~ operator
        /// </summary>
        /// <param name="v1"></param>
        /// <returns></returns>
        public static VAL operator ~(VAL v1)
        {
            
            if (v1.ty == VALTYPE.hostcon)
                return HostFunction.OperatorOverloading(Operator.op_OnesComplement, v1, null, true);

            if (v1.value is byte)
                return VAL.Boxing1(~(byte)v1.value);
            if (v1.value is sbyte)
                return VAL.Boxing1(~(sbyte)v1.value);
            if (v1.value is short)
                return VAL.Boxing1(~(short)v1.value);
            if (v1.value is ushort)
                return VAL.Boxing1(~(ushort)v1.value);
            if (v1.value is int)
                return VAL.Boxing1(~(int)v1.value);
            if (v1.value is uint)
                return VAL.Boxing1(~(uint)v1.value);
            if (v1.value is long)
                return VAL.Boxing1(~(long)v1.value);
            if (v1.value is ulong)
                return VAL.Boxing1(~(ulong)v1.value);

            throw new TieException("invalid operation from ~{0}", v1);
        }

        /// <summary>
        /// bitwise or operator, support operator overloading in host object
        /// </summary>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        /// <returns></returns>
        public static VAL operator |(VAL v1, VAL v2)
        {
            if (v1.isEnum && v2.isEnum)
                return HostOperation.EnumOperation(v1, v2, (int)v1.value | (int)v2.value);
            
            if (v1.ty == VALTYPE.hostcon)
                return HostFunction.OperatorOverloading(Operator.op_BitwiseOr, v1, v2, true);

            if (v1.value.GetType() == v2.value.GetType())
            {
                if (v1.value is byte)
                    return VAL.Boxing1((byte)v1.value | (byte)v2.value);
                if (v1.value is sbyte)
                    return VAL.Boxing1((sbyte)v1.value | (sbyte)v2.value);
                if (v1.value is short)
                    return VAL.Boxing1((short)v1.value | (short)v2.value);
                if (v1.value is ushort)
                    return VAL.Boxing1((ushort)v1.value | (ushort)v2.value);
                if (v1.value is uint)
                    return VAL.Boxing1((uint)v1.value | (uint)v2.value);
                if (v1.value is long)
                    return VAL.Boxing1((long)v1.value | (long)v2.value);
                if (v1.value is ulong)
                    return VAL.Boxing1((ulong)v1.value | (ulong)v2.value);
            }
            throw new TieException("invalid operation from {0} | {1}", v1, v2);

        }

        /// <summary>
        /// bitwise and operator, support operator overloading in host object
        /// </summary>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        /// <returns></returns>
        public static VAL operator &(VAL v1, VAL v2)
        {
            if (v1.isEnum && v2.isEnum)
                return HostOperation.EnumOperation(v1, v2, (int)v1.value & (int)v2.value);

            if (v1.ty == VALTYPE.hostcon)
                return HostFunction.OperatorOverloading(Operator.op_BitwiseAnd, v1, v2, true);

            if (v1.value.GetType() == v2.value.GetType())
            {
                if (v1.value is byte)
                    return VAL.Boxing1((byte)v1.value & (byte)v2.value);
                if (v1.value is sbyte)
                    return VAL.Boxing1((sbyte)v1.value & (sbyte)v2.value);
                if (v1.value is short)
                    return VAL.Boxing1((short)v1.value & (short)v2.value);
                if (v1.value is ushort)
                    return VAL.Boxing1((ushort)v1.value & (ushort)v2.value);
                if (v1.value is uint)
                    return VAL.Boxing1((uint)v1.value & (uint)v2.value);
                if (v1.value is long)
                    return VAL.Boxing1((long)v1.value & (long)v2.value);
                if (v1.value is ulong)
                    return VAL.Boxing1((ulong)v1.value & (ulong)v2.value);
            }

            throw new TieException("invalid operation from {0} & {1}", v1, v2);
            
        }

        /// <summary>
        /// bitwise xor operator, support operator overloading in host object
        /// </summary>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        /// <returns></returns>
        public static VAL operator ^(VAL v1, VAL v2)
        {
            if (v1.isEnum && v2.isEnum)
                return HostOperation.EnumOperation(v1, v2, (int)v1.value ^ (int)v2.value);
        
            if (v1.ty == VALTYPE.hostcon)
                return HostFunction.OperatorOverloading(Operator.op_ExclusiveOr, v1, v2, true);

            if (v1.value.GetType() == v2.value.GetType())
            {
                if (v1.value is byte)
                    return VAL.Boxing1((byte)v1.value ^ (byte)v2.value);
                if (v1.value is sbyte)
                    return VAL.Boxing1((sbyte)v1.value ^ (sbyte)v2.value);
                if (v1.value is short)
                    return VAL.Boxing1((short)v1.value ^ (short)v2.value);
                if (v1.value is ushort)
                    return VAL.Boxing1((ushort)v1.value ^ (ushort)v2.value);
                if (v1.value is uint)
                    return VAL.Boxing1((uint)v1.value ^ (uint)v2.value);
                if (v1.value is long)
                    return VAL.Boxing1((long)v1.value ^ (long)v2.value);
                if (v1.value is ulong)
                    return VAL.Boxing1((ulong)v1.value ^ (ulong)v2.value);
            }

            throw new TieException("invalid operation from {0} ^ {1}", v1, v2);
        }

        private bool isBitwiseValue
        {
            get
            {
                return ty == VALTYPE.intcon || value is uint || value is long || value is ulong;
            }
        }

        private bool isEnum
        {
            get
            {
                return value is int || Type.IsEnum;
            }
        }

        #endregion



        #region v = exp1 << int, v=exp1>> int

        /// <summary>
        /// bitwise left shift operator, support operator overloading in host object
        /// </summary>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        /// <returns></returns>
        public static VAL operator <<(VAL v1, int v2)
        {
            if (v1.value is byte)
                return VAL.Boxing1((byte)v1.value << v2);
            if (v1.value is sbyte)
                return VAL.Boxing1((sbyte)v1.value << v2);
            if (v1.value is short)
                return VAL.Boxing1((short)v1.value << v2);
            if (v1.value is ushort)
                return VAL.Boxing1((ushort)v1.value << v2);
            if (v1.value is int)
                return VAL.Boxing1((int)v1.value << v2);
            if (v1.value is uint)
                return VAL.Boxing1((uint)v1.value << v2);
            if (v1.value is long)
                return VAL.Boxing1((long)v1.value << v2);
            if (v1.value is ulong)
                return VAL.Boxing1((ulong)v1.value << v2);

            if (v1.ty == VALTYPE.hostcon)
                return HostFunction.OperatorOverloading(Operator.op_LeftShift, v1, new VAL(v2), true);

            throw new TieException("invalid operation from {0} << {1}", v1, v2);
        }

        /// <summary>
        /// bitwise right shift operator, support operator overloading in host object
        /// </summary>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        /// <returns></returns>
        public static VAL operator >>(VAL v1, int v2)
        {
            if (v1.value is byte)
                return VAL.Boxing1((byte)v1.value >> v2);
            if (v1.value is sbyte)
                return VAL.Boxing1((sbyte)v1.value >> v2);
            if (v1.value is short)
                return VAL.Boxing1((short)v1.value >> v2);
            if (v1.value is ushort)
                return VAL.Boxing1((ushort)v1.value >> v2);
            if (v1.value is int)
                return VAL.Boxing1((int)v1.value >> v2);
            if (v1.value is uint)
                return VAL.Boxing1((uint)v1.value >> v2);
            if (v1.value is long)
                return VAL.Boxing1((long)v1.value >> v2);
            if (v1.value is ulong)
                return VAL.Boxing1((ulong)v1.value >> v2);


            if (v1.ty == VALTYPE.hostcon)
                return HostFunction.OperatorOverloading(Operator.op_RightShift, v1, new VAL(v2), true);

            throw new TieException("invalid operation from {0} >> {1}", v1, v2);
        }
        #endregion


        #region Operator []
        //for C# programming use
        /// <summary>
        /// returns i-th element of list
        /// </summary>
        /// <param name="pos"></param>
        /// <returns></returns>
        public VAL this[int pos]
        {
            get
            {
                VAL arr = new VAL(pos);
                return getter(arr, false, OffsetType.ANY);
            }
            set
            {
                VAL arr = new VAL(pos);
                setter(arr, value, OffsetType.ARRAY);
            }
        }

        /// <summary>
        /// returns value by key in associative array
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public VAL this[string key]
        {
            get
            {
                VAL arr = new VAL(key);
                return getter(arr, false, OffsetType.ANY);
            }
            set
            {
                VAL arr = new VAL(key);
                setter(arr, value, OffsetType.STRUCT);
            }
        }

        /// <summary>
        /// returns offset of value, either property or array of host object
        /// </summary>
        /// <param name="arr"></param>
        /// <returns></returns>
        public VAL this[VAL arr]
        {
            get
            {
                return getter(arr, true, OffsetType.ANY);
            }
            set
            {
                setter(arr, value, OffsetType.ANY);
            }
        }


        internal VAL getter(VAL arr, bool created, OffsetType offsetType)
        {
            if (ty == VALTYPE.hostcon)
            {
                return HostOperation.HostTypeOffset(this, arr, offsetType);
            }

            if (ty != VALTYPE.listcon)
            {
                 return VAL.VOID;
            }

    
            return ((VALL)value).getter(arr, created);
        }

        private void setter(VAL arr, VAL value, OffsetType offsetType)
        {
            switch (arr.ty)
            {
                case VALTYPE.stringcon:
                    string key = arr.Str;
                    if (ty == VALTYPE.nullcon)
                    {
                        ty = VALTYPE.listcon;
                        this.value = new VALL();
                    }

                    if (ty == VALTYPE.hostcon)
                    {
                        this.temp = new HostOffset( this, key );
                        HostOperation.HostTypeAssign(this, value);
                        return;
                    }

                    if (ty != VALTYPE.listcon)
                        return;

                    ((VALL)this.value)[key] = value;

                    return;

                case  VALTYPE.intcon :
                    int pos = arr.Intcon;
                    if (ty == VALTYPE.nullcon)
                    {
                        ty = VALTYPE.listcon;
                        this.value = new VALL();
                    }

                    if (ty != VALTYPE.listcon)
                        return;

                    ((VALL)this.value)[pos] = value;

                    return;

                case VALTYPE.listcon:
                    return;
            }
            
            return;
      }
    


        #endregion VAL Operator

     
        #region VAL properties

        /// <summary>
        /// returns string value
        /// </summary>
        public string Str
        {
            get
            {
                return (string)value;
            }
            set
            {
                ty = VALTYPE.stringcon;
                this.value = value;
            }
        }

        internal int Address
        {
            get
            {
                return (int)value;
            }
            set
            {
                this.value = value;
            }

        }

        /// <summary>
        /// returns integet value
        /// </summary>
        public int Intcon
        {
            get
            {
                return (int)value;
            }
            set
            {
                ty = VALTYPE.intcon;
                this.value = value;
            }
        }


        /// <summary>
        /// returns double value
        /// </summary>
        public double Doublecon
        {
            get
            {
                return (double)value;
            }
            set
            {
                ty = VALTYPE.doublecon;
                this.value = value;
            }
        }

        /// <summary>
        /// returns decimal value
        /// </summary>
        public decimal Decimalcon
        {
            get
            {
                return (decimal)value;
            }
            set
            {
                ty = VALTYPE.decimalcon;
                this.value = value;
            }
        }

        /// <summary>
        /// returns boolean value
        /// </summary>
        public bool Boolcon
        {
            get
            {
                return (bool)value;
            }
            set
            {
                ty = VALTYPE.boolcon;
                this.value = value;
            }
        }

   
        internal VALL List
        {
            get
            {
                return (VALL)value;
            }

        }

        /// <summary>
        /// returns number of elements of list, if not list, returns -1
        /// </summary>
        public int Size
        {
            get
            {
                if (ty == VALTYPE.listcon)
                    return ((VALL)value).Size;
                else
                    return -1;
            }

        }

    
        /// <summary>
        /// value is defined
        /// </summary>
        public bool Defined        { get { return ty != VALTYPE.voidcon; }}

        /// <summary>
        /// value is not defined
        /// </summary>
        public bool Undefined      { get { return ty == VALTYPE.voidcon; }}

        /// <summary>
        /// is null?
        /// </summary>
        public bool IsNull         { get { return ty == VALTYPE.nullcon; }}

        /// <summary>
        /// is boolean?
        /// </summary>
        public bool IsBool         { get { return ty == VALTYPE.boolcon; }}

        /// <summary>
        /// is host type object?
        /// </summary>
        public bool IsHostType     { get { return ty == VALTYPE.hostcon; }}

        /// <summary>
        /// is integer?
        /// </summary>
        public bool IsInt          { get { return ty == VALTYPE.intcon;  }}

        /// <summary>
        /// is list value?
        /// </summary>
        public bool IsList         { get { return ty == VALTYPE.listcon; }}

        /// <summary>
        /// is double?
        /// </summary>
        public bool IsDouble       { get { return ty == VALTYPE.doublecon; }}

        /// <summary>
        /// is decimal?
        /// </summary>
        public bool IsDecimal      { get { return ty == VALTYPE.decimalcon; }}

        /// <summary>
        /// is user defined function?
        /// </summary>
        public bool IsFunction     { get { return ty == VALTYPE.funccon; }}

        /// <summary>
        /// is user defined class?
        /// </summary>
        public bool IsClass        { get { return ty == VALTYPE.classcon;}}

        /// <summary>
        /// is boolean and true?
        /// </summary>
        public bool IsTrue         { get { return ty == VALTYPE.boolcon && this.Boolcon; }}

        /// <summary>
        /// is boolean and false?
        /// </summary>
        public bool IsFalse        { get { return ty == VALTYPE.boolcon && !this.Boolcon; }}
        

 
        #endregion


        #region multiple dimensional array

        /// <summary>
        /// Make multiple dimension array
        /// </summary>
        /// <param name="args">ranks</param>
        /// <returns></returns>
        public static VAL Array(params int[] args)
        {
            if (args.Length == 0)
                return new VAL(new VALL());

            return makeArray(0, args);
        }

        private static VAL makeArray(int pos, int[] args)
        {

            if (pos == args.Length)
                return new VAL();

            int n = 0;
            try
            {
                n = (int)args[pos];
            }
            catch (Exception)
            {
                return makeArray(pos + 1, args);
            }

            VALL L = new VALL();
            for (int i = 0; i < n; i++)
                L.Add(makeArray(pos + 1, args));

            return new VAL(L);
        }

        #endregion

        #region Associative Array

        /// <summary>
        /// is associative array?
        /// </summary>
        /// <returns></returns>
        public bool IsAssociativeArray()
        {
            if (ty != VALTYPE.listcon)
                return false;

            return ((VALL)value).IsAssociativeArray();
        }


        /// <summary>
        /// Add element into list
        /// </summary>
        /// <param name="val"></param>
        public void Add(VAL val)
        {
            if (ty == VALTYPE.nullcon)
            {
                ty = VALTYPE.listcon;
                value = new VALL();
            }
                
            ((VALL)value).Add(val);
        }

        /// <summary>
        /// add a key value pair to assoicative array
        /// </summary>
        /// <param name="key"></param>
        /// <param name="obj"></param>
        public void Add(string key, object obj)
        {
            if (ty == VALTYPE.nullcon)
            {
                ty = VALTYPE.listcon;
                value = new VALL();
            }

            ((VALL)value).Add(key, Boxing1(obj));
        }

        /// <summary>
        /// returns object array
        /// </summary>
        public object[] ObjectArray
        {
            get
            {
                return ((VALL)value).ObjectArray;
            }
        }

        #endregion


        #region VAL ToString
        
        /// <summary>
        ///   Converts the value of this instance to a System.String. display all format
        /// </summary>
        /// <returns> A string whose value is the same as this instance.</returns>
        public override String ToString()
        {
            return encode(true, true, false);
        }

        
        /// <summary>
        /// display to end user
        /// </summary>
        /// <returns></returns>
        public String ToString2()
        {
            return encode(false, true, false);
        }

        //display instuctions/assembly
        internal String ToString3()
        {
            return encode(true, false, false);
        }

        /// <summary>
        /// returns instance string used for persistent purpose
        /// </summary>
        public String Valor
        {
            get
            {
                return encode(true, true, true);
            }
        }

        internal String encode(bool quotationMark, bool nullMark, bool persistent)
        {
            StringWriter o = new StringWriter();
            string s;

            switch (ty)
            {
                case VALTYPE.voidcon:
                    o.Write("void");
                    break;

                case VALTYPE.nullcon:
                    if (nullMark)
                        o.Write("null");
                    break;

                case VALTYPE.intcon:
                    if (value is int)
                        o.Write(value);
                    else if (persistent)
                    {
                        if (value is byte)
                            return string.Format("(byte){0}", value);
                        if (value is short)
                            return string.Format("(short){0}", value);
                        if (value is long)
                            return string.Format("(long){0}", value);
                    }
                    break;

                case VALTYPE.decimalcon:
                case VALTYPE.doublecon:
                    o.Write(value);
                    if (value is double)
                    {
                        if (Math.Ceiling((double)value) == (double)value)
                            o.Write(".0");
                    }
                    else if (persistent)
                    {
                        if (value is decimal)
                            return string.Format("(decimal){0}", value);
                        if (value is float)
                            return string.Format("(float){0}", value);
                    }
                    break;

                case VALTYPE.boolcon: o.Write("{0}", (bool)value ? "true" : "false"); break;


                case VALTYPE.listcon:
                    if (Class == null)
                        o.Write(((VALL)value).encode(quotationMark, nullMark, persistent));
                    else
                        o.Write("{0}.typeof(\"{1}\")", ((VALL)value).encode(quotationMark, nullMark, persistent), Class);
                    break;

                case VALTYPE.hostcon:  // .NET object
                    if (value is DateTime)
                    {
                        DateTime d = (DateTime)value;
                        if (quotationMark)
                            o.Write("DateTime({0},{1},{2},{3},{4},{5})", d.Year, d.Month, d.Day, d.Hour, d.Minute, d.Second);
                        else
                            o.Write("#{0}#", value);
                    }
                    else
                        o.Write(HostCoding.Encode(this.value, persistent));
                    break;

                case VALTYPE.stringcon:
                case VALTYPE.scriptcon:      
                    if (value is char)
                    {
                        o.Write("'{0}'", value);
                        break;
                    }
                    if (ty == VALTYPE.stringcon && quotationMark)
                        o.Write("\"");

                    s = (string)value;
                    for (int i = 0; i < s.Length; i++)
                    {
                        switch (s[i])
                        {
                            case '"':
                                o.Write(ty == VALTYPE.stringcon && quotationMark ? "\\\"" : "\"");
                                break;

                            case '\\':
                                o.Write(ty == VALTYPE.stringcon && quotationMark ? "\\\\" : "\\");
                                break;

                            case '\n':
                                o.Write(ty == VALTYPE.stringcon && quotationMark ? "\\n" : "\n");
                                break;

                            case '\t':
                                o.Write(ty == VALTYPE.stringcon && quotationMark ? "\\t" : "\t");
                                break;

                            default:
                                o.Write(s[i]);
                                break;
                        }

                    }

                    if (ty == VALTYPE.stringcon && quotationMark)
                        o.Write("\"");
                    break;

                case VALTYPE.funccon:
                    o.Write("{0}({1},{2})", Constant.FUNC_FUNCTION, new VAL(Class), VAL.Boxing1(value));
                    if (persistent)
                    {
                        //VAL v = Module.encode(Library.GetModule(Class));
                    }
                    break;
                case VALTYPE.classcon:
                    o.Write("{0}({1},{2})", Constant.FUNC_CLASS, new VAL(Class), VAL.Boxing1(value));
                    if (persistent)
                    {
                        //o.Write(Library.GetModule(Class));
                    }
                    break;

                case VALTYPE.identcon:
                    o.Write(value);
                    break;

                default:
                    o.Write("(ty={0} value={1})", ty, value);
                    break;
          
            }
            return o.ToString();
        }



        #endregion


        #region ToJson, ToXml

        /// <summary>
        /// convert instance to JSON
        /// </summary>
        /// <returns></returns>
        public string ToJson()
        {
            return Export.ToJson(this, "", false);
        }

        /// <summary>
        /// convert instance to JSON
        /// </summary>
        /// <param name="tag">root tag</param>
        /// <returns>JSON string</returns>
        public string ToJson(string tag)
        {
            return Export.ToJson(this, tag, true);
        }

        /// <summary>
        /// Convert instance to XML
        /// </summary>
        /// <param name="tag"></param>
        /// <returns></returns>
        public string ToXml(string tag)
        {
            return Export.ToXml(this, tag); 
        }

       

        #endregion


        #region ICollection

        private List<VAL> __LIST
        {
            get
            {
                if (ty != VALTYPE.listcon)
                    throw new TieException(string.Format("VAL {0} is not list.", this));

                FieldInfo fieldInfo = value.GetType().GetField("list", BindingFlags.NonPublic | BindingFlags.Instance);
                return (List<VAL>)fieldInfo.GetValue(this.value);
            }
            
        }

        /// <summary>
        /// returns count of list
        /// </summary>
        public int Count { get  { return this.Size;  } }

        /// <summary>
        /// Returns an enumerator that iterates
        /// </summary>
        /// <returns></returns>
        public IEnumerator<VAL> GetEnumerator()
        {
            return __LIST.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return __LIST.GetEnumerator();
        }

        /// <summary>
        /// Copies the entire list to a compatible one-dimensional
        /// array, starting at the specified index of the target array.
        /// </summary>
        /// <param name="array"></param>
        /// <param name="arrayIndex"></param>
        public void CopyTo(VAL[] array, int arrayIndex)
        {
            __LIST.CopyTo(array, arrayIndex);
        }

        /// <summary>
        /// always returns true
        /// </summary>
        public bool IsReadOnly
        {
            get
            {
                return false;
            }
        }
        
        /// <summary>
        /// clear list
        /// </summary>
        public void Clear()
        {
            if (ty == VALTYPE.listcon)
                List.Clear();
        }

        /// <summary>
        /// list contains item?
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public bool Contains(VAL item)
        {
            if (ty == VALTYPE.listcon)
                return List.Contains(item);
            else
                return false;
        }
        
        /// <summary>
        /// removes item from list
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public bool Remove(VAL item)
        {
            if (ty == VALTYPE.listcon)
                return List.Remove(item);
            return false;
        }

        #endregion


        #region property HostValue, Type
        
        /// <summary>
        /// returns Host value
        /// </summary>
        public object HostValue
        {
            get
            {
                if (ty == VALTYPE.nullcon)
                    return null;

                if (ty == VALTYPE.funccon)   
                    return this;             

                if (ty != VALTYPE.listcon)
                    return this.value;

                if (Class == "VAL")     //cast to VAL, e.g. function VAL(..), skip HostValue unboxing if val.Class=="VAL"
                    return this;

                if (Class != null)     
                {
                    Type type = HostType.GetType(Class);
                    if (type != null)
                        this.List.ty = type;
                }

                return this.List.HostValue;
            }
        }

        internal Type Type
        {
            get
            {
                if (ty == VALTYPE.nullcon)
                {
                    if (value is Type)
                        return (Type)value;   
                    else
                        return typeof(object);
                }
                else if (ty == VALTYPE.voidcon)
                {
                    return typeof(object);
                }

                if (ty == VALTYPE.listcon && List.ty != null)
                    return List.ty;

                return HostValue.GetType();
            }

            set
            {
                if (ty == VALTYPE.nullcon)         
                    this.value = value;

                else if (ty == VALTYPE.listcon)
                {
                    List.ty = value;
                    this.Class = value.FullName;
                }
            }
        }

        #endregion


        #region CAST

        internal static VAL cast(VAL val, Type type)
        {

            object value = val.value;
            if (val.ty == VALTYPE.nullcon)
            {
                val.Type = type;
                val.Class = type.UnderlyingSystemType.FullName;
                return val;
            }
          
            else if (val.ty == VALTYPE.listcon && type.IsArray)
            {
                val.List.ty = type;
                val.Class = type.FullName;
                return val;
            }
            else if (val.ty == VALTYPE.hostcon)
            {
                string func = Operator.op_Explicit.ToString();
                MethodInfo method = HostFunction.methodof(value, type, func, new Type[] { value.GetType() });
                if (method != null)
                {
                    VALL L = new VALL();
                    L.Add(val);

                    HostFunction hFunc = new HostFunction(value, func, L);
                    return hFunc.RunFunction(new MethodInfo[] { method });
                }
            }

            if (type == typeof(object))
                return val;

            if (!(value is IConvertible))
                throw new TieException("cannot cast {0} to Type [{1}]", value, type.FullName);

            if (type == typeof(bool))
            {
                val.ty = VALTYPE.boolcon;
                val.value = Convert.ToBoolean(value);
            }

            else if (type == typeof(byte))
            {
                val.ty = VALTYPE.intcon;
                val.value = Convert.ToByte(value);
            }
            else if (type == typeof(sbyte))
            {
                val.ty = VALTYPE.intcon;
                val.value = Convert.ToSByte(value);
            }
            else if (type == typeof(short))
            {
                val.ty = VALTYPE.intcon;
                val.value = Convert.ToInt16(value);
            }
            else if (type == typeof(ushort))
            {
                val.ty = VALTYPE.intcon;
                val.value = Convert.ToUInt16(value);
            }
            else if (type == typeof(int))
            {
                val.ty = VALTYPE.intcon;
                val.value = Convert.ToInt32(value);
            }
            else if (type == typeof(uint))
            {
                val.ty = VALTYPE.decimalcon;
                val.value = Convert.ToUInt32(value);
            }
            else if (type == typeof(long))
            {
                val.ty = VALTYPE.decimalcon;
                val.value = Convert.ToInt64(value);
            }
            else if (type == typeof(ulong))
            {
                val.ty = VALTYPE.decimalcon;
                val.value = Convert.ToUInt64(value);
            }

            else if (type == typeof(float))
            {
                val.ty = VALTYPE.doublecon;
                val.value = Convert.ToSingle(value);
            }
            else if (type == typeof(double))
            {
                val.ty = VALTYPE.doublecon;
                val.value = Convert.ToDouble(value);
            }
            else if (type == typeof(decimal))
            {
                val.ty = VALTYPE.decimalcon;
                val.value = Convert.ToDecimal(value);
            }

            else if (type == typeof(char))
            {
                val.ty = VALTYPE.stringcon;
                val.value = Convert.ToChar(value);
            }
            else if (type == typeof(string))
            {
                val.ty = VALTYPE.stringcon;
                val.value = Convert.ToString(value);
            }


            else if (type == typeof(DateTime))
                val.value = Convert.ToDateTime(value);
       

#if !SILVERLIGHT
            else
                val.value = Convert.ChangeType(value, type);
#endif

            return val;
        }

        #endregion


        #region Basic Type implicit/explict bool/string/int/double/decimal, (object[])/(Memory)

#if IMPLICIT
        /*
         * Usage:
         * 
         *  VAL b1 = true;
         *  : VAL b1 = new VAL(true);
         * 
         **/
        public static implicit operator VAL(bool x)
        {
            return new VAL(x);
        }
        public static implicit operator VAL(string x)
        {
            return new VAL(x);
        }
        public static implicit operator VAL(int x)
        {
            return new VAL(x);
        }

        public static implicit operator VAL(double x)
        {
            return new VAL(x);
        }

        public static implicit operator VAL(decimal x)
        {
            return VAL.Boxing1(x);
        }

        public static implicit operator VAL(Array x)
        {
            return VAL.Boxing(x);
        }
#endif

        /*
         * Usage:
         * 
         *  VAL b1 = new VAL(true);
         *   bool b2 = (bool)b1;
         * 
         **/
        /// <summary>
        /// explicit convert to boolean
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        public static explicit operator bool(VAL x)
        {
            if (x.ty != VALTYPE.boolcon) throw new InvalidCastException();
            return (bool)x.value;
        }

        /// <summary>
        /// explicit convert to string
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        public static explicit operator string(VAL x)
        {
            if (x.ty != VALTYPE.stringcon) throw new InvalidCastException();
            return (string)x.value;
        }

        /// <summary>
        /// explicit convert to int
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        public static explicit operator int(VAL x)
        {
            if (x.ty != VALTYPE.intcon) throw new InvalidCastException();
            return (int)x.value;
            //return (int)VAL.cast(x, typeof(int)).value;
        }

        /// <summary>
        /// explicit convert to double
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        public static explicit operator double(VAL x)
        {
            if (x.ty != VALTYPE.doublecon) throw new InvalidCastException();
            return (double)x.value;
        }

        /// <summary>
        /// explicit convert to decimal
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        public static explicit operator decimal(VAL x)
        {
            if (x.ty != VALTYPE.decimalcon) throw new InvalidCastException();
            return (decimal)x.value;
        }

       /// <summary>
        /// explicit convert to object array
       /// </summary>
       /// <param name="x"></param>
       /// <returns></returns>
        public static explicit operator object[](VAL x)
        {
            if (x.ty != VALTYPE.listcon) throw new InvalidCastException();

            return x.ObjectArray;
        }

        /// <summary>
        /// explicit convert to varible dictionary
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        public static explicit operator Memory(VAL x)
        {
            return new Memory(x);
        }

        #endregion


   
        #region Extend Type implicit/explict  DateTime/DBNull,  byte/sbyte/short/ushort/uint/float/char/long/ulong

    
#if IMPLICIT
        public static implicit operator VAL(char x)
        {
            return VAL.Boxing1(x);
        }

        public static implicit operator VAL(byte x)
        {
            return VAL.Boxing1(x);
        }
        public static implicit operator VAL(sbyte x)
        {
            return VAL.Boxing1(x);
        }


        public static implicit operator VAL(short x)
        {
            return VAL.Boxing1(x);
        }
        public static implicit operator VAL(ushort x)
        {
            return VAL.Boxing1(x);
        }

        public static implicit operator VAL(uint x)
        {
            return VAL.Boxing1(x);
        }

        public static implicit operator VAL(long x)
        {
            return VAL.Boxing1(x);
        }

        public static implicit operator VAL(ulong x)
        {
            return VAL.Boxing1(x);
        }

        public static implicit operator VAL(float x)
        {
            return VAL.Boxing1(x);
        }

     

        public static implicit operator VAL(DateTime x)
        {
            return VAL.NewHostType(x);
        }

        public static implicit operator VAL(DBNull x)
        {
            return VAL.Boxing1(DBNull.Value);
        }
        //-------------------------------------------------------------
#endif

     
        /// <summary>
        /// explicit convert to char
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        public static explicit operator char(VAL x)
        {
            return (char)VAL.cast(x, typeof(char)).value;
        }

        /// <summary>
        /// explicit convert to byte
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        public static explicit operator byte(VAL x)
        {
            return (byte)VAL.cast(x, typeof(byte)).value;
        }

        /// <summary>
        /// explicit convert to sbyte
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        public static explicit operator sbyte(VAL x)
        {
            return (sbyte)VAL.cast(x, typeof(sbyte)).value;
        }

        /// <summary>
        /// explicit convert to short
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        public static explicit operator short(VAL x)
        {
            return (short)VAL.cast(x, typeof(short)).value;
        }

        /// <summary>
        /// explicit convert to ushort
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        public static explicit operator ushort(VAL x)
        {
            return (ushort)VAL.cast(x, typeof(ushort)).value;
        }

        /// <summary>
        /// explicit convert to uint
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        public static explicit operator uint(VAL x)
        {
            return (uint)VAL.cast(x, typeof(uint)).value;
        }

        /// <summary>
        /// explicit convert to long
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        public static explicit operator long(VAL x)
        {
            return (long)VAL.cast(x, typeof(long)).value;
        }

        /// <summary>
        /// explicit convert to ulong
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        public static explicit operator ulong(VAL x)
        {
            return (ulong)VAL.cast(x, typeof(ulong)).value;
        }

        /// <summary>
        /// explicit convert to float
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        public static explicit operator float(VAL x)
        {
            return (float)VAL.cast(x, typeof(float)).value;
        }

        /// <summary>
        /// explicit convert to DateTime
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        public static explicit operator DateTime(VAL x)
        {
            return (DateTime)VAL.cast(x, typeof(DateTime)).value;
        }

        /// <summary>
        /// explicit convert to System.DBNull
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        public static explicit operator DBNull(VAL x)
        {
            if (x.ty == VALTYPE.nullcon || x.value is DBNull)
                return System.DBNull.Value;
            else
                throw new TieException("cannot cast value {0} to System.DBNull", x);
        }

      

        #endregion

    }


}
