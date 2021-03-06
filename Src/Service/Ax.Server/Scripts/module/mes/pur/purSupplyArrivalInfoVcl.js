﻿purSupplyArrivalInfoVcl = function () {
    Ax.vcl.LibVclGrid.apply(this, arguments);
};
var proto = purSupplyArrivalInfoVcl.prototype = Object.create(Ax.vcl.LibVclGrid.prototype);
proto.constructor = purSupplyArrivalInfoVcl;
proto.vclHandler = function (sender, e) {
    Ax.vcl.LibVclData.prototype.vclHandler.apply(this, arguments);
    switch (e.libEventType) {
        case LibEventTypeEnum.ButtonClick:
            if (e.dataInfo.fieldName == "BtnPrint") {
                var items = this.dataSet.getTable(0).getRange();
                function prn_Preview() {
                    CreatePrintPage();
                    LODOP.PREVIEW(); //打印预览	
                    //LODOP.PRINT();
                };
                function CreatePrintPage() {
                    LODOP = getLodop(document.getElementById('LODOP'), document.getElementById('LODOP_EM'));
                    LODOP.PRINT_INITA(0, 0,"100%", "100%", "打印控件功能演示_Ext");
                    AddText();
                    LODOP.ADD_PRINT_TEXT(3, "80%", 135, 20, "总页号：第#页/共&页");
                    LODOP.SET_PRINT_STYLEA(0, "FontSize", 11);
                    LODOP.SET_PRINT_STYLEA(0, "ItemType", 2);//页号项
                    LODOP.SET_PRINT_STYLEA(0, "Horient", 1); //右边距锁定
                    //LODOP.ADD_PRINT_TEXT(3, 34, 196, 20, "总页眉：美的MES系统");
                    //LODOP.SET_PRINT_STYLEA(0, "FontSize", 11);
                    //LODOP.SET_PRINT_STYLEA(0, "ItemType", 1);//页眉页脚
                }
                function AddText() {
                    LODOP.ADD_PRINT_TEXT(25, "40%", 355, 30, "供应商到货信息表");
                    LODOP.SET_PRINT_STYLEA(1, "FontSize", 16);
                    LODOP.SET_PRINT_STYLEA(1, "Bold", 1);
                    //设置表格样式
                    var strTableStyle = "<style type='text/css'>table{width:'100%';border-collapse: collapse;} table thead td b{font-size: 25px;} table tr td{font-size: 13px;} table tfoot td{font-size: 15px;}</style>";
                    //将数据拼成一个table  
                    var strTableStartHtml = "<table border='1' width='100%' bordercolor='#336699' cellpadding='0' cellspacing='0' align='center'>";
                    var strTableEndHtml = "</table>";
                    var strTableTheadHtml = "<thead style='height: 30px' bgcolor='#efefef'>";
                    var strTableTrHtml = "";
                    strTableTheadHtml += "<td nowrap align='center' style=font-size: 15px><b>供应商名称</b></td>";
                    strTableTheadHtml += "<td nowrap align='center' style=font-size: 15px><b>物料名称</b></td>";
                    strTableTheadHtml += "<td nowrap align='center' style=font-size: 15px><b>仓管责任人描述</b></td>";
                    strTableTheadHtml += "<td nowrap align='center' style=font-size: 15px width = '120px'><b>送货单号</b></td>";
                    strTableTheadHtml += "<td nowrap align='center' style=font-size: 15px><b>计划送货量</b></td>"; 
                    strTableTheadHtml += "<td nowrap align='center' style=font-size: 15px width = '120px'><b>条码号</b></td>";  
                    strTableTheadHtml += "</thead>";
                    var zjeTotal = 0;
                    for (var i = 0; i < items.length; i++) {
                        var td = "<tr style='height: 60px'><td align='center'>";
                        td += items[i].data["SUPPLIERNAME"];
                        td += "</td><td align='center'>";
                        td += items[i].data["MATERIALID"];
                        td += "</td><td align='center'>";
                        td += items[i].data["STKPERSONIDDESC"];
                        td += "</td><td align='center'>";
                        td += items[i].data["DELIVERYNOTENO"];
                        td += "</td><td align='center'>";
                        td += items[i].data["PLANQTY"];
                        td += "</td><td align='center'><div id = Barcode" + i + "></div>";
                        td += "</td></tr>";
                        strTableTrHtml += td;
                    }
                    var strTableTfoot = "<tr style='height: 30px'><td align='center'><b>合计</b></td><td>&nbsp;</td><td>&nbsp;</td><td align='right'><b>测试</b>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;</td><td>&nbsp;</td><td>&nbsp;</td></tr>";
                    var strPageFooter = "<tfoot style='height: 30px'><td align='center'><b>本页合计</b></td><td tdata='pageNO' format='#' align='center'><p align='center'><b>第<font color='#0000FF'>#</font>页</b></p></td><td tdata='pageCount' format='#' align='center'><p align='center'><b>总<font color='#0000FF'>#</font>页</b></td><td>&nbsp;</td><td>&nbsp;</td><td>&nbsp;</td></tfoot>";
                    $("#printBarcode").append(strTableStartHtml + strTableTheadHtml + strTableTrHtml + strPageFooter + strTableEndHtml);
                    for (var i = 0; i < items.length; i++) {
                        $("#Barcode" + i).barcode({ code: items[i].data["BARCODE"], crc: true, rect: true }, "128B", { barWidth: 1, barHeight: 40, showHRI: false });
                    }
                    LODOP.ADD_PRINT_HTM(50, 0, 800, 1000, strTableStyle);
                    LODOP.ADD_PRINT_TABLE(75, 0, "100%", "85%", $("#printBarcode")[0].innerHTML);
                };
                prn_Preview();
            }
            break;
    }
}
