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
using System.Text;
using System.IO;
using System.Reflection;


namespace AxCRL.Parser
{


    class CPU 
    {
        Context context;

        //memory
        Instruction[]           CS;	    //Code segment
        StackSegment<VAL>        SS;	    //Stack segment
        StackSegment<VAL>        ES;	   
        StackSegment<int>        EX;       

        //registers
        Register REG;
        int BP;
        int SI;
        int IP;

        VAL R0, R1;

        //internal use
        private static VAL Mark = new VAL("$MaRk#_!`@=+|_x.?:%-;'*#&><");		//	 used in List or Array ({Mark) (}End)

        private string scope;
        private string moduleName;
        private Position position;

        public CPU(Module module, Context context)
        {
            this.context = context;

            this.scope = "";
            this.moduleName = module.moduleName;

            position = new Position(module.moduleName, null);
            


            SS = new StackSegment<VAL>(Constant.MAX_STACK);
            for (int i = 0; i < SS.Size; i++) 
                SS[i] = new VAL();

            ES = new StackSegment<VAL>(Constant.MAX_EXTRA);
            for (int i = 0; i < ES.Size; i++)
                ES[i] = new VAL();

            EX = new StackSegment<int>(Constant.MAX_EXCEPTION);
            REG = new Register(SS);

            IP = module.IP1;
            BP = 0;
            SI = 0;

            CS = module.CS;

        }

        public Position Position
        {
            get
            {
                return this.position;
            }
        }


 
        public VAL Run(int breakPoint)
        {
        L1:
     
            Instruction I = CS[IP];
            Operand operand = I.operand;

            position.line = I.line;
            position.col = I.col;
            position.cur = I.cur;
            position.block = I.block;

            if (I.line == breakPoint)
                return null;

            switch (I.cmd)
            {
                //----------------------------------------------------------------------------	

                #region  MOV, STO,STO1, RMT. RCP

                case INSTYPE.MOV:
                    if (operand.ty == OPRTYPE.identcon)		    // MOV [v]
                    {
                        if (CS[IP + 1].cmd == INSTYPE.OFS)  // if this is offset of a struct, keep variable name
                            REG.Push(new VAL(operand));
                        else
                            REG.Push(GetVAL(operand.Str, false));
                    }
                    else if (operand.ty == OPRTYPE.addrcon)	// MOV [BP-3]
                    {
                        //VAL opr = Register.BPAddr(BP, operand);

                        VAL opr = new VAL();
                        opr.ty = VALTYPE.addrcon;
                        opr.value = BP + operand.Addr;
                        opr.name = operand.name;

                        REG.Push(opr);
                    }
                    else
                    {
                        VAL x = new VAL(operand);
                        if (operand.ty == OPRTYPE.funccon)
                        {
                            if (ES.SP > -1)
                                x.temp = new ContextInstance(this.context, ES.Top()) ;
                            else
                                x.temp = new ContextInstance(this.context, new VAL());
                        }

                        REG.Push(VAL.Clone(x)); // MOV 3
                    }
                    break;


    

                //----------------------------------------------------------------------------	


                case INSTYPE.STO1: 
                case INSTYPE.STO: 
                    R0 = REG.Pop(); 
                    R1 = REG.pop();
                    if (R1.ty == VALTYPE.addrcon)
                        SS[R1.Address] = R0;
                    else
                        HostOperation.Assign(R1, R0);

                    if (I.cmd == INSTYPE.STO)
                        REG.Push(R1);
                    break;

                case INSTYPE.RMT:
                    if (CS[IP + 1].cmd != INSTYPE.HALT)  //used for expression decoding,keep last value 
                        REG.Pop();
                    break;

                case INSTYPE.RCP:
                    REG.Push(VAL.Clone(REG.Top()));
                    break;
                
                #endregion


               //----------------------------------------------------------------------------	

                #region PUSH/POP/SP/ESI/ESO
                case INSTYPE.ESI:
                    R0 = REG.Pop();
                    ES.Push(R0);
                    break;
                case INSTYPE.ESO:
                    ES.Pop();
                    break;

                case INSTYPE.PUSH:
                    if (operand != null && operand.ty == OPRTYPE.regcon)
                        switch (operand.SEG)
                        {
                            case SEGREG.BP: SS.Push(new VAL(BP)); break;
                            case SEGREG.SP: SS.Push(new VAL(SS.SP)); break;
                            case SEGREG.SI: SS.Push(new VAL(SI)); break;
                            case SEGREG.IP: SS.Push(new VAL(IP + 2)); break;
                            case SEGREG.EX: EX.Push((int)operand.value); break;
                        }
                    else
                    {
                        R0 = REG.Pop();
                        SS.Push(R0);
                    }
                    break;

                case INSTYPE.POP:
                    if (operand.ty == OPRTYPE.regcon)
                        switch (operand.SEG)
                        {
                            case SEGREG.BP: BP = (SS.Pop()).Address; break;
                            case SEGREG.SI: SI = (SS.Pop()).Address; break;
                            case SEGREG.SP: SS.SP = (SS.Pop()).Address; break;
                            case SEGREG.EX: EX.Pop(); break;
                        }
                    else
                    {
                        R0 = SS.Pop();
                        REG.Push(R0);
                    }
                    break;
                case INSTYPE.SP:
                    SS.SP += operand.Addr;
                    break;

                #endregion


                //----------------------------------------------------------------------------	

                #region OFS, ARR
                //----------------------------------------------------------------------------	
                // Associative List
                // Mike={{"street", "3620 Street"},{"zip", 20201},{"phone","111-222-333},{"apt",1111}}
                // Mike.street = "3620 Street";
                // Mike.zip = 40802;
                case INSTYPE.OFS:
                    R0 = REG.Pop();
                    R1 = REG.pop();
                    {
                        VAL v = new VAL();
                        switch (R1.ty)
                        {
                            case VALTYPE.hostcon:
                                v = R1.getter(R0, true, OffsetType.STRUCT);
                                goto LOFS;
                            case VALTYPE.addrcon:
                                v = SS[R1.Address];
                                if (v.Undefined || v.IsNull)       
                                {
                                    v.ty = VALTYPE.listcon;
                                    v.value = new VALL();
                                }
                                break;
                            case VALTYPE.listcon:
                                v = R1;
                                break;
                            default:   // if assoicative arrary is empty or not list
                                R1.ty = VALTYPE.listcon;
                                R1.value = new VALL();
                                v = R1;
                                break;
                        }

                        switch (v.ty)
                        {
                            case VALTYPE.listcon:
                                VALL L = v.List;
                                v = L[R0.Str];

                                // if property is not found in the associative array
                                if (!v.Defined)
                                    L[R0.Str] = v;

                                 break;

                            case VALTYPE.hostcon:
                                //VAL.Assign(v, VAL.HostTypeOffset(v.value, R0.value));
                                v = HostOperation.HostTypeOffset(v, R0, OffsetType.STRUCT); 
                                break;
                        }

                    LOFS:
                        if ((object)v == null)
                            v = new VAL();

                        v.name = R1.name + "." + R0.name;      
                        REG.Push(v);
                    }
                    break;

                case INSTYPE.ARR:
                    R0 = REG.Pop();
                    R1 = REG.pop(); 
                    {
                        VAL v = new VAL();
                        switch (R1.ty)
                        {
                            case VALTYPE.addrcon:
                                v = SS[R1.Address];   //indirect addressing
                                if (v.Undefined || v.IsNull)  
                                {
                                    v.ty = VALTYPE.listcon;
                                    v.value = new VALL();
                                    v = v.getter(R0, true, OffsetType.ARRAY);
                                }
                                else
                                    v = v.getter(R0, true, OffsetType.ARRAY);
                                break;

                            case VALTYPE.listcon:               //push reference
                                v = R1.getter(R0, true, OffsetType.ARRAY);
                                break;
                            case VALTYPE.hostcon:
                                v = R1.getter(R0, true, OffsetType.ARRAY);
                                if (!v.Defined)
                                    throw new RuntimeException(position, "{0} does not have property {1}.", R1, R0); 
                                break;
                            default:
                                //refer: public VAL this[VAL arr], when R1 == null, dynamically allocate a list
                                R1.ty = VALTYPE.listcon;
                                R1.value = new VALL();
                                v = R1.getter(R0, true, OffsetType.ARRAY);

                                //JError.OnRuntime(0);
                                break;
                        }
                        
                        v.name = R1.name + "[" + R0.ToString() + "]";
                        REG.Push(v);
                    }
                    break;
                
                #endregion


                //----------------------------------------------------------------------------	

                #region  CALL, NEW, ENDP, RET, GNRC
                case  INSTYPE.GNRC:
                    R0 = REG.Pop();     // R0 = new Type[] { string, int }
                    R1 = REG.Pop();     // R1 = System.Collection.Generic
                    {
                        Operand opr = I.operand;
                        VAL R2 = R1[opr.Str];     // I.val.Str == Dictionary`2
                        if (R2.Undefined)             // Type is not registered
                        {
                            object t = HostType.GetType(R1.name + "." + opr.Str);
                            if (t != null)
                                R2 = VAL.NewHostType(t);
                            else
                                throw new RuntimeException(position, "Type {0}.{1} is not registered.", R1.name, opr.Str);
                        }
                        
                        object A = R0.HostValue;
                        if (A is object[] && ((object[])A).Length == 0)     //case: typeof(Dictionary<,>)
                        {
                            if (R2.value is Type)
                                REG.Push(VAL.NewHostType(R2.value));
                            else
                                throw new RuntimeException(position, "{0} is not System.Type.", R1);
                        }
                        else
                        {
                            if (!(A is Type[]))
                                throw new RuntimeException(position, "<{0}> is not System.Type[].", R0.ToString2());

                            if (R2.value is Type)
                            {
                                Type t = (Type)R2.value;
                                REG.Push(VAL.NewHostType(t.MakeGenericType((Type[])A)));
                            }
                            else if (R2.value is MethodInfo)
                            {
                                MethodInfo t = (MethodInfo)R2.value;
                                VAL m = VAL.NewHostType(t.MakeGenericMethod((Type[])A));
                                m.temp = R2.temp;
                                REG.Push(m);
                            }
                            else if (R2.value is MethodInfo[])
                            {
                                MethodInfo[] T = (MethodInfo[])R2.value;
                                for (int i = 0; i < T.Length; i++)
                                    T[i] = T[i].MakeGenericMethod((Type[])A);
                                VAL m = VAL.NewHostType(T);
                                m.temp = R2.temp;
                                REG.Push(m);
                            }
                            else
                                throw new RuntimeException(position, "{0} is not System.Type.", R1);
                        }
                    }
                    
                    break;
                    
                case INSTYPE.CALL:
                    if (operand.ty == OPRTYPE.intcon)          
                        IP = operand.Addr;
                    else if (operand.ty == OPRTYPE.addrcon)  // high-level programming 
                    {
                        SysFuncCallByAddr(SS[operand.Addr + BP]);
                    }
                    else if (operand.ty == OPRTYPE.regcon)  
                    {
                        SysFuncCallByAddr(SS[operand.Addr + SS.SP]);
                    }
                    else if (operand.ty == OPRTYPE.none)   //used for Generic method
                    {
                        if (ES.IsEmpty())         
                            R0 = REG.Pop();
                        else
                            R0 = ES.Top();          

                        SysFuncCallByName(R0);
                    }
                    else
                        SysFuncCallByName(new VAL(operand));              
                    goto L1;

                case INSTYPE.NEW:
                    if (operand.ty == OPRTYPE.funccon)
                    {
                        NewInstance(new VAL(operand));     //system predifined class & user-defined class
                    }
                    else if (operand.ty == OPRTYPE.none)   //used for Generic class
                    {
                        if (ES.IsEmpty())           
                            R0 = REG.Pop();
                        else
                            R0 = ES.Top();          

                        NewInstance(R0);
                    }
                    else if (operand.ty == OPRTYPE.intcon)
                    {
                        int opr = (int)operand.value;
                        if (opr > 1)    
                        {
                            R0 = REG.Pop();
                            R1 = REG.Pop();
                        }
                        else
                        {
                            R1 = REG.Pop();
                        }

                        if (R1.value is Type)
                        {
                            Type ty = (Type)R1.value;

                            if (opr == 1)      
                            {
                                if (ty.IsArray)
                                    R0 = VAL.Array();
                                else
                                    R0 = new VAL();
                            }

                            if (R0.ty == VALTYPE.listcon)
                            {
                                if (ty.IsArray)
                                    R0.List.ty = ty;
                                else
                                    throw new RuntimeException(position, "new object failed. {0} is not Array Type", R1);
                            }

                            R0.Class = ty.FullName;
                            REG.Push(R0);
                        }
                        else
                            throw new RuntimeException(position, "new object failed. {0} is not System.Type", R1);

                        IP++;

                    }

                    goto L1;

                case INSTYPE.ENDP: 
                    if ((OPRTYPE)operand.Intcon == OPRTYPE.classcon)
                        REG.Push(ES.Top());	        //return this;
                    else
                        REG.Push(VAL.VOID);    //return void;

                    SS.SP = BP;                 //PUSH BP; POP SP;
                    BP = (SS.Pop()).Address;    //POP BP;
                    
                    R0 = SS.Top();              //EXEC RET
                    IP = R0.Address;		    
                    goto L1;
                case INSTYPE.RET:
                    if (!SS.IsEmpty())
                    {
                        R0 = SS.Top();
                        IP = R0.Address;		    //goto V.i
                    }
                    else
                    {
                        if (REG.IsEmpty())
                            return VAL.VOID;
                        else
                            return REG.Top();
                    }

                    goto L1;
                
                #endregion

                
                //----------------------------------------------------------------------------	

                #region  +,-,*,/,%,>,<,!=,==,<=,>=, ++, --
                //----------------------------------------------------------------------------	
                case INSTYPE.NEG: 
                    R0 = REG.Pop();
                    if (operand.Intcon == -1)
                        R0 = -R0;   //call VAL.operator -(VAL)
                    else
                        R0 = +R0;   //call VAL.operator +(VAL)
                    REG.Push(R0);  
                    break;
                case INSTYPE.ADD: R0 = REG.Pop(); R1 = REG.Pop(); R1 += R0; REG.Push(R1); break;
                case INSTYPE.SUB: R0 = REG.Pop(); R1 = REG.Pop(); R1 -= R0; REG.Push(R1); break;
                case INSTYPE.MUL: R0 = REG.Pop(); R1 = REG.Pop(); R1 *= R0; REG.Push(R1); break;
                case INSTYPE.DIV: R0 = REG.Pop(); R1 = REG.Pop(); R1 /= R0; REG.Push(R1); break;
                case INSTYPE.MOD: R0 = REG.Pop(); R1 = REG.Pop(); R1 %= R0; REG.Push(R1); break;

                case INSTYPE.GTR: R0 = REG.Pop(); R1 = REG.Pop(); REG.Push(new VAL(R1 > R0)); break;
                case INSTYPE.LSS: R0 = REG.Pop(); R1 = REG.Pop(); REG.Push(new VAL(R1 < R0)); break;
                case INSTYPE.GEQ: R0 = REG.Pop(); R1 = REG.Pop(); REG.Push(new VAL(R1 >= R0)); break;
                case INSTYPE.LEQ: R0 = REG.Pop(); R1 = REG.Pop(); REG.Push(new VAL(R1 <= R0)); break;
                case INSTYPE.EQL: R0 = REG.Pop(); R1 = REG.Pop(); REG.Push(new VAL(R1 == R0)); break;
                case INSTYPE.NEQ: R0 = REG.Pop(); R1 = REG.Pop(); REG.Push(new VAL(R1 != R0)); break;

                case INSTYPE.INC: 
                    R0 = REG.pop();
                    R1 = VAL.Clone(R0); 
                    if (R0.ty == VALTYPE.addrcon)
                        SS[R0.Address] += new VAL(1);
                    else //global varible
                        HostOperation.Assign(R0, R0 + new VAL(1));
                    REG.Push(R1);
                    break;
                case INSTYPE.DEC: 
                    R0 = REG.pop();
                    R1 = VAL.Clone(R0);
                    if (R0.ty == VALTYPE.addrcon)
                        SS[R0.Address] -= new VAL(1);
                    else //global varible
                        HostOperation.Assign(R0, R0 - new VAL(1));
                    REG.Push(R1);
                    break;
                
                #endregion


                //----------------------------------------------------------------------------	

                #region MARK, END, EACH, CAS, DIRC

                case INSTYPE.MARK: REG.Push(Mark); break;
                case INSTYPE.END:
                    {
                        VAL L = new VAL(new VALL());
                        while (REG.Top() != Mark)
                            L.List.Insert(REG.Pop());
                        REG.Pop();		// pop mark
                        REG.Push(L);
                    } break;
                
                case INSTYPE.EACH:
                    R0 = REG.Pop();         //Collection
                    R1 = REG.pop();         //address of element [BP+addr]
                    REG.Push(ForEach(R0, SS[R1.Intcon], SS[R1.Intcon + 1]));
                    break;
      
                case INSTYPE.CAS:
                    R0 = REG.Pop(); R1 = REG.Top();
                    if (R1 == R0) { REG.Pop(); IP = operand.Addr; goto L1; } //goto V.i
                    break;

                case INSTYPE.DIRC:      //directive command
                    switch (operand.mod)
                    {
                        case Constant.SCOPE:
                            this.scope = (string)operand.value;
                            break;

                    }
                    break;
                
                #endregion


                //----------------------------------------------------------------------------	

                #region &&, ||, ~, &, |, >>, <<
                //----------------------------------------------------------------------------	
                case INSTYPE.NOTNOT: 
                    R0 = REG.Pop();
                    if (R0.ty == VALTYPE.stringcon)
                        REG.Push(Computer.Run(scope, R0.Str, CodeType.statements, context));
                    else
                        REG.Push(new VAL(!R0));     //call VAL.operator !(VAL)
                    break;

                case INSTYPE.ANDAND: R0 = REG.pop(); R1 = REG.Pop(); 
                    REG.Push(new VAL(R0.ty == VALTYPE.boolcon && R1.ty == VALTYPE.boolcon? R0.Boolcon && R1.Boolcon: false)); 
                    break;
                case INSTYPE.OROR: R0 = REG.pop(); R1 = REG.Pop();
                    REG.Push(new VAL(R0.ty == VALTYPE.boolcon && R1.ty == VALTYPE.boolcon ? R0.Boolcon || R1.Boolcon : false));
                    break;


                //----------------------------------------------------------------------------	
                case INSTYPE.NOT: R0 = REG.Pop();
                    REG.Push(~R0);  //call VAL.operator ~(VAL)
                    break;
                case INSTYPE.AND: R0 = REG.pop(); R1 = REG.Pop();
                    REG.Push(R1 & R0);
                    break;
                case INSTYPE.OR: R0 = REG.pop(); R1 = REG.Pop();
                    REG.Push(R1 | R0 );
                    break;
                case INSTYPE.XOR: R0 = REG.pop(); R1 = REG.Pop();
                    REG.Push(R1 ^ R0);
                    break;

                //----------------------------------------------------------------------------	
                case INSTYPE.SHL: R0 = REG.pop(); R1 = REG.Pop();
                    if (!(R0.value is int))
                        throw new RuntimeException(position, "the 2nd operand {0} in << operation must be integer", R0);
                    REG.Push(R1 << (int)R0.value);
                    break;
                case INSTYPE.SHR: R0 = REG.pop(); R1 = REG.Pop();
                    if (!(R0.value is int))
                        throw new RuntimeException(position, "the 2nd operand {0} in >> operation must be integer", R0);
                    REG.Push(R1 >> (int)R0.value);
                    break;
                #endregion


                //----------------------------------------------------------------------------	
                
                #region JUMP, ADR, VLU

                case INSTYPE.JMP: IP = operand.Addr; goto L1;
                case INSTYPE.LJMP: IP += operand.Addr; goto L1;

                case INSTYPE.JNZ:
                    if (REG.Pop() == new VAL(true))
                    { IP = operand.Addr; goto L1; }
                    else break;
                case INSTYPE.JZ:
                    if (REG.Pop() != new VAL(true))
                    { IP = operand.Addr; goto L1; }
                    else break;
                case INSTYPE.LJZ:
                    if (REG.Pop() != new VAL(true))
                    { IP += operand.Addr; goto L1; }
                    else break;

                case INSTYPE.ADR:
                    R0 = REG.Pop();
                    REG.Push(new VAL(R0.name));
                    break;

                case INSTYPE.VLU:
                    R0 = REG.Pop();
                    if (R0.ty == VALTYPE.stringcon)
                        REG.Push(Computer.Run(scope, R0.Str,CodeType.expression, context));
                    else
                        throw new RuntimeException(position, "Invalid address type:" + R0.ToString()); 
                    break;
                
                #endregion


                //----------------------------------------------------------------------------	


                #region  THIS,BASE
                case INSTYPE.THIS:                                       //this.x
                    if (ES.SP > -1)                 
                    {
                        if (this.scope != "")       
                        {
                            VAL NS = GetScopeVAL(this.scope);
                            if (NS != ES.Top())
                                REG.Push(NS);
                        }
                        else
                            REG.Push(ES.Top());
                    }
                    else if (this.scope != "")
                    {
                        if (operand.ty == OPRTYPE.intcon)                     //this.x=100;
                            REG.Push(GetScopeVAL(this.scope));
                        else
                            REG.Push(new VAL(this.scope));         //p=&this.x;
                    }
                    else
                    {
                        if (CS[IP + 2].cmd == INSTYPE.OFS)
                            CS[IP + 2].cmd = INSTYPE.NOP;
                        else
                            throw new RuntimeException(position, "Operator[this] is invalid since namespace is empty.");
                    }
                    break;

                case INSTYPE.BASE:       //base.x
                    //if (ES.SP > -1)                     
                    //{
                    //    VAL BS = ES.Top()[JExpression.BASE_INSTANCE];
                    //    if((object)BS!=null)
                    //        REG.Push(BS);
                    //    else
                    //        throw new RuntimeException(string.Format("Operator[base] is invalid since class {0} does not have base class.",ES.Top()));
                    //}
                    if (this.scope != "")
                    {
                        string bv = "";
                        int n = 1;
                        if (operand.ty == OPRTYPE.intcon)      //base.base.x=10;
                        {
                            n = operand.Intcon;

                            bv = GetBaseScope(this.scope, n);

                            if (bv != "")
                                REG.Push(GetScopeVAL(bv));
                            else
                            {
                                if (CS[IP + 2].cmd == INSTYPE.OFS)
                                    CS[IP + 2].cmd = INSTYPE.NOP;
                            }
                        }
                        else
                        {
                            n = operand.Addr;              // p = &base.base.x;
                            bv = GetBaseScope(this.scope, n);
                            REG.Push(new VAL(bv));
                        }
                    }
                    else
                        throw new RuntimeException(position, "Operator[base] is invalid since scope is root.");
                    break;

                #endregion


                //----------------------------------------------------------------------------	

                #region THRW, DDT, HALT

                case INSTYPE.THRW:
                    R0 = REG.Top();
                    if (!(R0.HostValue is Exception))
                        throw new RuntimeException(position, "{0} is not type of System.Exception.", R0.HostValue);

                    if (EX.IsEmpty())  
                        throw (Exception)R0.HostValue;
                    IP = EX.Pop();  

                    if (IP == -1)   
                        throw (Exception)R0.HostValue;
                    break;

                case INSTYPE.DDT:
                    Logger.WriteLine(DebugInfo()); 
                    break;
                case INSTYPE.HALT:
                    if (REG.IsEmpty())
                        return VAL.VOID;
                    else
                        return REG.Top();

                #endregion

            }

            IP++;
            goto L1;
        }

       

        #region Functions parameters/arguments

        private VALL SysFuncBeginParameter()
        {
            int count = SysFuncBegin();
            VALL L = new VALL();
            for (int i = 0; i < count; i++)
                L.Add(SysFuncParam(i));
            
            return L;
        }

        //return number of arguments passed in
        private int SysFuncBegin()
        {
            SS.Push(new VAL(BP));		// push BP
            BP = SS.SP;				    // mov BP,SP
            int num = -CS[IP + 1].operand.Addr - 1;	//IV[IP+1] == SP -n, total number of parameters
            return num;
        }

        //set returning value
        private bool SysFuncEnd(VAL ret)
        {
            REG.Push(ret);		// return value

            SS.SP = BP;				    // mov SP,BP
            IP = SS[BP - 1].Address;	// mov IP, (return address)
            BP = SS.Pop().Address;		// pop BP
            
            return true;
        }

        //get ith argument
        private VAL SysFuncParam(int i)
        {
            return SS[BP - 2 - i];      // skip PUSH IP, CALL 
        }

        #endregion


        #region CALL SysFuncCall(..)

        bool SysFuncCallByAddr(VAL proc)
        {
            VAL instance = new VAL();
            bool arg0 = !ES.IsEmpty() && CS[IP + 2].cmd == INSTYPE.ESO;  

            if (ES.SP > -1)          
                instance = ES.Top(); 


            return SysFuncCall(proc.value, proc, instance, arg0);
        }



        bool SysFuncCallByName(VAL name)
        {
            if (name.value is int)
                return SysFuncCallByAddr(name);
            else if(name.value is MethodInfo || name.value is MethodInfo[]) 
                return SysFuncCall(null, name, new VAL(), false);



            string func = name.Str;
            VAL proc = new VAL();
            VAL instance = new VAL();
            bool arg0 = !ES.IsEmpty() && CS[IP + 2].cmd == INSTYPE.ESO;    

            if (ES.SP > - 1 )                     //user-defined delegate .e.g  system.math.add= function(a,b) {return a+b;};   
            {
                instance = ES.Top();
                if (instance.ty == VALTYPE.listcon)
                {
                    proc = instance[func];
                    if (proc.ty == VALTYPE.funccon)
                    {
                        UserFuncCall(proc, instance, false);
                        return true;
                    }
                }
            }
            
            proc = GetVAL(func, true);
            
            return SysFuncCall(func, proc, instance, arg0);
        }


        private bool SysFuncCall(object func, VAL proc, VAL instance, bool arg0)
        {
            if (proc.ty == VALTYPE.funccon)
            {
                UserFuncCall(proc, instance, arg0);
                return true;
            }

            VALL L = SysFuncBeginParameter();

            if (arg0 && proc.temp is HostOffset)
            {
                object host = ((HostOffset)proc.temp).host;
                if (host != instance.value)     
                {
                    L.Insert(instance);       //extend method
                    arg0 = false;             
                }
            }
            VAL ret = HostOperation.HostTypeFunction(proc, L);
            if (ret.Defined)
                return SysFuncEnd(ret);



            if (arg0)
                L.Insert(instance);           
            try
            {
                ret = context.InvokeFunction((string)func, new VAL(L), position);
            }
            catch (FunctionNotFoundException e1)
            {
                throw e1;

            }
            catch (Exception e2)
            {
                if (e2 is RuntimeException)
                    throw e2;
                else
                    throw new RuntimeException(position, "Error: function {0}({1}) implementation, {2}", func, L.ToString2(), e2.Message);
            }

            return SysFuncEnd(ret);
        }


        private void UserFuncCall(VAL func, VAL instance, bool arg0)
        {
            int count1 = -CS[IP + 1].operand.Addr - 1;  

            if (moduleName == func.Class)               
            {
                if (arg0)     
                {
                    VAL ip = SS.Pop();
                    SS.Push(instance);
                    SS.Push(ip); 
                    count1++;
                }

                IP = (int)func.value;
                paramsCheck(IP, count1);

            }
            else  
            {
                VALL L = SysFuncBeginParameter();
                if (arg0) L.Insert(instance);
                VAL ret = InternalUserFuncCall(func, instance, new VAL(L));

                SysFuncEnd(ret);
            }

        }

        private void paramsCheck(int call, int count1)
        {
            int count2 = CS[call].operand.Intcon -1;      

            int diff = count1 - count2;
            if (diff == 0)
                return;

            if (diff < -1 || (diff > 0 && count2 == 0))     
                throw new RuntimeException(position, "Number of function arguments is not matched in module=[{0}] address=[{1}].",
                    moduleName, call);
            else if (diff >= -1)
                paramsArray(count1, count2); 
            
        }

        private void paramsArray(int argc1, int argc2)
        {

            int diff = argc1 - argc2;
            
            VAL retAddr = SS.Pop();                 

            VALL L1 = new VALL();
            for (int i = 0; i < argc2 - 1; i++)
                L1.Insert(SS.Pop());

            VALL L2 = new VALL();
            for (int i = 0; i <= diff; i++)
                L2.Add(SS.Pop());
            SS.Push(new VAL(L2));

            for (int i = 0; i < argc2 - 1; i++)
                SS.Push(L1[i]);
            
            SS.Push(retAddr);

            CS[retAddr.Intcon].operand.Addr += diff;
        }

        #endregion


        
        #region InternalUserFuncCall(..)


        public VAL InternalUserFuncCall(int address, VAL instance, VAL arguments)  
        {
           // if (func.ty != VALTYPE.funccon && func.ty!=VALTYPE.classcon)
           //     return null;
           // int address = (int)func.value;

            int BP1 = BP;
            int IP1 = IP;
            int SP1 = SS.SP;

            
            ES.Push(instance);

            int count1 = arguments.Size;
            for(int i=0; i<count1; i++)
                SS.Push(arguments[count1 - i -1]);

            IP = CS.Length-2;
            SS.Push(new VAL(IP));                                        
            CS[IP] = new Instruction(INSTYPE.SP, new Operand(-count1 - 1), Position.UNKNOWN);  
            CS[IP + 1] = new Instruction(INSTYPE.HALT, Position.UNKNOWN);       

            //function entry
            IP = address;                                                   //delegate PROC address
            paramsCheck(IP, count1);

            VAL ret;
#if DEBUG
            ret = Run(-1);                                             
#else
            try
            {
                ret = Run(-1);                                              
            }
            catch (Exception e)
            {
                if (e is PositionException)
                    throw e;
                else
                    throw new RuntimeException(position, "failed to invoke {0}({1}) {2}", CS[address].operand, arguments.List.ToString2(), e);
            }
#endif
            REG.Pop();                                                 
            ES.Pop();
            BP = BP1;
            IP = IP1;
            SS.SP = SP1;
            return ret;
        }


        public VAL InternalUserFuncCall(VAL func, VAL instance, VAL arguments)
        {
            if (func.Class != moduleName)
                return ExternalUserFuncCall(func, instance, arguments, this.context);  
            else
                return InternalUserFuncCall((int)func.value, instance, arguments); 
        }
           
        public static VAL ExternalUserFuncCall(VAL func, VAL instance, VAL arguments, Context context)
        {
            string moduleName = func.Class;
            Module module = Library.GetModule(moduleName);
            if (module == null)
                throw new TieException("Module is not loaded yet. " + moduleName);

            CPU cpu = new CPU(module, context);
            VAL ret = cpu.InternalUserFuncCall((int)func.value, instance, arguments);

            return ret;
        }

        #endregion



        #region New Instance

 
        bool NewInstance(VAL V)         //new user defined class, .net object or a listcon
        {
            VALL L = SysFuncBeginParameter();

            if (V.ty == VALTYPE.funccon)
            {
                string func = V.Str;


                VAL userClass = GetVAL(func, true); 
                if (userClass.ty == VALTYPE.classcon) 
                {
                    //VAL instance =  NewVAL.UserType(func);
                    VAL instance = new VAL();  

                    VAL ret = InternalUserFuncCall(userClass, instance, new VAL(L));   
                    return SysFuncEnd(ret);
                }
                else
                {
                    VAL args = new VAL(L);

                    VAL scope = new VAL();
                    if (!ES.IsEmpty())
                        scope = ES.Top();

                    VAL Clss = HostCoding.Decode(func, args, scope, context);      
                    return SysFuncEnd(Clss);
                }
            }
            else if (V.value is Type)       //generic class
            {
                VAL args = new VAL(L);
                object instance = Activator.CreateInstance((Type)V.value, HostCoding.ConstructorArguments(args));
                return SysFuncEnd(VAL.NewHostType(instance));
            }

            throw new RuntimeException(position, "new instance {0} failed", V);
        }
        
        



    
      
        
    #endregion



        #region Data Segment Management

  
        private VAL GetScopeVAL(string scope)
        {
            VAL THIS = new VAL();
            if (scope == "")
                return THIS;

            string[] L = Module.ParseScope(scope);
            THIS = GetVAL(L[0], false);
            
            int i = 1;
            while (i < L.Length)
            {
                string id = L[i];
                if (!THIS[id].Defined) 
                    THIS[id] = new VAL();
                THIS = THIS[id];

                i++;
            }
            
            THIS.name = scope;
            return THIS;
        }

        private VAL GetVAL(string ident, bool readOnly)
        {
            if (ES.SP > -1)     
            {
                VAL val = null;
                VAL instance = ES.Top();
                if(instance.ty == VALTYPE.hostcon && CS[IP + 2].cmd == INSTYPE.ESO)
                    val = HostOperation.HostTypeOffset(instance, new VAL(ident), OffsetType.STRUCT);
                else
                    val = instance[ident];

                if (val.Defined)
                    return val;
            }

            return context.GetVAL(ident, readOnly);
        }

        private static string GetBaseScope(string scope, int n)
        {
            
            char[] a = scope.ToCharArray();
            int i = a.Length-1;
            while (i >0)
            {
                if (a[i] == '.')
                    n--;
                
                if(n==0)
                    break;
                
                i--;
            }

            return new string(a,0,i);
        }

        #endregion


        private VAL ForEach(VAL collection, VAL element, VAL index)
        {
            int i = index.Intcon;

            if (collection.ty == VALTYPE.listcon)
            {
                if (i >= collection.Size)
                    return new VAL(false);

                HostOperation.Assign(element, collection[i]);
                index.value = ++i;
                return new VAL(true);
            }

            else if (collection.ty == VALTYPE.hostcon)
            {
                if (!(collection.value is IEnumerable))
                    throw new RuntimeException(position, "foreach statement requires {0} to be IEnumerable", collection.value.GetType().FullName);

                IEnumerable enumerable = (IEnumerable)collection.value;
                
                IEnumerator enumerator;
                if (index.temp is IEnumerator)
                    enumerator = (IEnumerator)index.temp;
                else
                {
                    enumerator = enumerable.GetEnumerator();
                    index.temp = enumerator;
                }
                
                if (enumerator.MoveNext())
                {
                    HostOperation.Assign(element, VAL.Boxing1(enumerator.Current));
                    index.value = ++i;  
                    return new VAL(true);
                }
                else
                {
                    index.temp = null;
                    return new VAL(false);
                }
            }
            else
                throw new RuntimeException(position, "foreach statement requires [{0}]={1} to be IEnumerable", collection.name, collection.ToString());

        }

        public string DebugInfo()
        {
            StringWriter b = new StringWriter();
            b.WriteLine("mod:" + moduleName);
            b.WriteLine(CS[IP]);
            b.WriteLine(string.Format("IP={0}\tBP={1}", IP, BP));
            b.WriteLine(REG.ToString());
            b.WriteLine("ES: " + ES.ToString());
            b.WriteLine("SS: " + SS.ToString());
            //b.WriteLine("DS1: " + DS1.ToString());
            //b.WriteLine("DS2: " + DS2.ToString());
            
            return b.ToString();
        }


    

    }
}
