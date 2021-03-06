﻿
finAPOtherInvoiceVcl = function () {
    Ax.vcl.LibVclData.apply(this, arguments);
    me = this;
};
var proto = finAPOtherInvoiceVcl.prototype = Object.create(Ax.vcl.LibVclData.prototype);
proto.constructor = finAPOtherInvoiceVcl;
var attId = 0;

proto.vclHandler = function (sender, e) {
    Ax.vcl.LibVclData.prototype.vclHandler.apply(this, arguments);
    switch (e.libEventType) {            
        case LibEventTypeEnum.Validated:
            var masterRow = this.dataSet.getTable(0).data.items[0];
            var bodyTable = this.dataSet.getTable(1);
            var standardCoilRate = masterRow.get("STANDARDCOILRATE");
            if (e.dataInfo.tableIndex == 0) {
                if(e.dataInfo.fieldName=="STANDARDCOILRATE")
                {
                    for(var i=0;i<bodyTable.data.items.length;i++)
                    {
                        debugger;
                        var detailData = bodyTable.data.items[i];
                        var conversion = this.GetData(detailData, standardCoilRate, e);
                        var data = this.invorkBcf('CalculateAmount', [conversion, 2, 0]);
                        this.SetData(detailData,data);
                    }
                }
                this.SumAmount(bodyTable, masterRow);
                if (e.dataInfo.fieldName == "INVOICETYPEID") {
                    for (var i = 0; i < bodyTable.data.items.length; i++) {
                        bodyTable.data.items[i].set("TAXRATE", masterRow.get("TAXRATE"));
                        var conversion = this.GetData(bodyTable.data.items[i], standardCoilRate, e);
                        var data = this.invorkBcf('CalculateAmount', [conversion, 1, 0]);
                        this.SetData(bodyTable.data.items[i], data);
                    }
                }
                if (e.dataInfo.fieldName == "TAXRATE") {
                    for (var i = 0; i < bodyTable.data.items.length; i++) {
                        bodyTable.data.items[i].set("TAXRATE", e.dataInfo.value);
                        var conversion = this.GetData(bodyTable.data.items[i], standardCoilRate, e);
                        var data = this.invorkBcf('CalculateAmount', [conversion, 1, 0]);
                        this.SetData(bodyTable.data.items[i], data);
                    }
                    this.SumAmount(bodyTable, masterRow);
                }
                if (e.dataInfo.fieldName == "CONTACTSOBJECTID") {
                    for (var i = 0; i < bodyTable.data.items.length; i++) {
                        bodyTable.data.items[i].set("TAXRATE", masterRow.get("TAXRATE"));
                        var conversion = this.GetData(bodyTable.data.items[i], standardCoilRate, e);
                        var data = this.invorkBcf('CalculateAmount', [conversion, 1, 0]);
                        data.StandardCoilRate  = masterRow.get("STANDARDCOILRATE");
                        data = this.invorkBcf('CalculateAmount', [data, 2, 0]);
                        this.SetData(bodyTable.data.items[i], data);
                    }
                    this.SumAmount(bodyTable, masterRow);
                }
            }
            if (e.dataInfo.tableIndex == 1) {          
                if(e.dataInfo.fieldName=="TAXRATE")
                {
                    var conversion = this.GetData(e.dataInfo.dataRow, standardCoilRate, e);
                    var data = this.invorkBcf('CalculateAmount', [conversion, 1, 0]);
                    this.SetData(e.dataInfo.dataRow,data);
                    this.SumAmount(this.dataSet.getTable(1), masterRow);
                }
                if (e.dataInfo.fieldName == "INVOICEPURQTY")
                {
                    var conversion = this.GetData(e.dataInfo.dataRow, standardCoilRate, e);
                    var data = this.invorkBcf('CalculateAmount', [conversion, 3, 0]);
                    this.SetData(e.dataInfo.dataRow, data);
                    this.SumAmount(this.dataSet.getTable(1), masterRow);
                }
                if (e.dataInfo.fieldName == "NOTAXPRICE") {
                    var conversion = this.GetData(e.dataInfo.dataRow, standardCoilRate, e);
                    var data = this.invorkBcf('CalculateAmount', [conversion, 5, 0]);
                    this.SetData(e.dataInfo.dataRow, data);
                    this.SumAmount(this.dataSet.getTable(1), masterRow);
                }
                if (e.dataInfo.fieldName == "TAXPRICE") {
                    var conversion = this.GetData(e.dataInfo.dataRow, standardCoilRate, e);
                    var data = this.invorkBcf('CalculateAmount', [conversion, 4, 0]);
                    this.SetData(e.dataInfo.dataRow, data);
                    this.SumAmount(this.dataSet.getTable(1), masterRow);
                }
                if (e.dataInfo.fieldName == "AMOUNT") {
                    var conversion = this.GetData(e.dataInfo.dataRow, standardCoilRate, e);
                    var data = this.invorkBcf('CalculateAmount', [conversion, 6, 0]);
                    this.SetData(e.dataInfo.dataRow, data);
                    this.SumAmount(this.dataSet.getTable(1), masterRow);
                }
                if (e.dataInfo.fieldName == "TAXAMOUNT") {
                    var conversion = this.GetData(e.dataInfo.dataRow, standardCoilRate, e);
                    var data = this.invorkBcf('CalculateAmount', [conversion, 7, 0]);
                    this.SetData(e.dataInfo.dataRow, data);
                    this.SumAmount(this.dataSet.getTable(1), masterRow);
                }
                if (e.dataInfo.fieldName == "BWAMOUNT") {
                    var conversion = this.GetData(e.dataInfo.dataRow, standardCoilRate, e);
                    var data = this.invorkBcf('CalculateAmount', [conversion, 8, 0]);
                    this.SetData(e.dataInfo.dataRow, data);
                    this.SumAmount(this.dataSet.getTable(1), masterRow);
                }
                if (e.dataInfo.fieldName == "BWTAXAMOUNT") {
                    var conversion = this.GetData(e.dataInfo.dataRow, standardCoilRate, e);
                    var data = this.invorkBcf('CalculateAmount', [conversion, 9, 0]);
                    this.SetData(e.dataInfo.dataRow, data);
                    this.SumAmount(this.dataSet.getTable(1), masterRow);
                }
                if (e.dataInfo.fieldName == "INVOICEPURQTY") {
                    e.dataInfo.dataRow.set("INVOICEQTY", e.dataInfo.value);
                }
                this.forms[0].updateRecord(masterRow);

                if (e.dataInfo.fieldName == "WAREINPURQTY") {
                    e.dataInfo.dataRow.set("WAREINQTY", e.dataInfo.value);
                }
                if (e.dataInfo.fieldName == "INVOICEPURQTY") {
                    e.dataInfo.dataRow.set("INVOICEQTY", e.dataInfo.value);
                }
                if (e.dataInfo.fieldName == "MATERIALID") {
                    var materialData = this.invorkBcf('GetMaterialData', [e.dataInfo.value]);
                    e.dataInfo.dataRow.set("UNITID", materialData.UnitId);
                    e.dataInfo.dataRow.set("UNITNAME", materialData.UnitName);
                    e.dataInfo.dataRow.set("DEALUNITID", materialData.UnitId);
                    e.dataInfo.dataRow.set("DEALUNITNAME", materialData.UnitName);
                }
            } 
            break;
        case LibEventTypeEnum.DeleteRow:
            var masterRow = this.dataSet.getTable(0).data.items[0];
            var bodyTable = this.dataSet.getTable(1);
            this.SumAmount(bodyTable, masterRow);
            break;
        case LibEventTypeEnum.AddRow:
            var masterRow = this.dataSet.getTable(0).data.items[0];
            e.dataInfo.dataRow.set("TAXRATE", masterRow.get("TAXRATE"));
            break;
        case LibEventTypeEnum.ColumnDbClick:
            if (e.dataInfo.tableIndex == 1) {
                bodyRow = e.dataInfo.dataRow;
            }
            if (e.dataInfo.fieldName == "ATTRIBUTENAME") {
                var MaterialId = e.dataInfo.dataRow.data["MATERIALID"];
                var AttributeId = e.dataInfo.dataRow.data["ATTRIBUTEID"];
                var AttributeCode = e.dataInfo.dataRow.data["ATTRIBUTECODE"];
                if (AttributeId != "") {
                    var returnData = this.invorkBcf('GetAttJson', [MaterialId, AttributeId, AttributeCode]);
                    var dataList = {
                        MaterialId: e.dataInfo.dataRow.data["MATERIALID"],
                        AttributeId: e.dataInfo.dataRow.data["ATTRIBUTEID"],
                        AttributeDesc: e.dataInfo.dataRow.data["ATTRIBUTEDESC"],
                        AttributeCode: e.dataInfo.dataRow.data["ATTRIBUTECODE"],
                        BillNo: e.dataInfo.dataRow.data["BILLNO"],
                        Row_Id: e.dataInfo.dataRow.data["ROW_ID"]
                    };
                    CreatAttForm(dataList, returnData, this, e, FillDataRow);
                }
            }
            break;
    }
}


proto.GetData = function (detailData, standardCoilRate, e) {
    var taxRate;
    var price;
    var quantity;
    var taxPrice;
    var amount;
    var taxAmount;
    var tax;
    var bwAmount;
    var bwTaxAmount;
    var bwTax;
    var standardcoilrate;
    if (e.dataInfo.fieldName == "TAXRATE") {
        taxRate = e.dataInfo.value;
    }
    else {
        taxRate = detailData.get("TAXRATE");
    }
    if (e.dataInfo.fieldName == "INVOICEPURQTY") {
        quantity = e.dataInfo.value;
    }
    else {
        quantity = detailData.get("INVOICEPURQTY");
    }

    if (e.dataInfo.fieldName == "NOTAXPRICE") {
        price = e.dataInfo.value;
    }
    else {
        price = detailData.get("NOTAXPRICE");
    }
    if (e.dataInfo.fieldName == "TAXPRICE") {
        taxPrice = e.dataInfo.value;
    }
    else {
        taxPrice = detailData.get("TAXPRICE");
    }
    if (e.dataInfo.fieldName == "AMOUNT") {
        amount = e.dataInfo.value;
    }
    else {
        amount = detailData.get("AMOUNT");
    }
    if (e.dataInfo.fieldName == "TAXAMOUNT") {
        taxAmount = e.dataInfo.value;
    }
    else {
        taxAmount = detailData.get("TAXAMOUNT");
    }
    if (e.dataInfo.fieldName == "TAX") {
        tax = e.dataInfo.value;
    }
    else {
        tax = detailData.get("TAX");
    }
    if (e.dataInfo.fieldName == "BWAMOUNT") {
        bwAmount = e.dataInfo.value;
    }
    else {
        bwAmount = detailData.get("BWAMOUNT");
    }
    if (e.dataInfo.fieldName == "BWTAXAMOUNT") {
        bwTaxAmount = e.dataInfo.value;
    }
    else {
        bwTaxAmount = detailData.get("BWTAXAMOUNT");
    }
    if (e.dataInfo.fieldName == "BWTAX") {
        bwTax = e.dataInfo.value;
    }
    else {
        bwTax = detailData.get("BWTAX");
    }
    if (e.dataInfo.fieldName == "STANDARDCOILRATE") {
        standardcoilrate = e.dataInfo.value;
    }
    else {
        standardcoilrate = standardCoilRate;
    }

    var conversion = {
        TaxRate: taxRate,
        StandardcoilRate: standardcoilrate,
        Quantity: quantity,
        taxPrice: taxPrice,
        Price: price,
        Amount: amount,
        TaxAmount: taxAmount,
        Tax: tax,
        BwAmount: bwAmount,
        BwTaxAmount: bwTaxAmount,
        BwTax: bwTax
    };
    return conversion;
}

proto.SetData = function (detailData, data) {
    detailData.set("TAXPRICE", data.TaxPrice);
    detailData.set("TAXRATE", data.TaxRate);
    detailData.set("NOTAXPRICE", data.Price);
    detailData.set("AMOUNT", data.Amount);
    detailData.set("TAXAMOUNT", data.TaxAmount);
    detailData.set("TAX", data.Tax);
    detailData.set("BWAMOUNT", data.BwAmount);
    detailData.set("BWTAXAMOUNT", data.BwTaxAmount);
    detailData.set("BWTAX", data.BwTax);
    detailData.set("INVOICEPURQTY", data.Quantity);
} 

proto.SumAmount = function (bodyTable, masterRow) {
    var bwTotalAmount = 0;
    var bwTotalTaxAmount = 0;
    var bwTotalTax = 0;
    var totalAmount = 0;
    var totalTaxAmount = 0;
    var totalTax = 0;
    for (var i = 0; i < bodyTable.data.items.length; i++) {
        var detailData = bodyTable.data.items[i];
        bwTotalAmount = detailData.get("BWAMOUNT") + bwTotalAmount;
        bwTotalTaxAmount = detailData.get("BWTAXAMOUNT") + bwTotalTaxAmount;
        bwTotalTax = detailData.get("BWTAX") + bwTotalTax;
        totalAmount = detailData.get("AMOUNT") + totalAmount;
        totalTaxAmount = detailData.get("TAXAMOUNT") + totalTaxAmount;
        totalTax = detailData.get("TAX") + totalTax;
    }
    //masterRow.set("BWTOTALAMOUNT", bwTotalAmount);
    //masterRow.set("BWTOTALTAXAMOUNT", bwTotalTaxAmount);
    //masterRow.set("BWTOTALTAX", bwTotalTax);
    //masterRow.set("TOTALAMOUNT", totalAmount);
    //masterRow.set("TOTALTAXAMOUNT", totalTaxAmount);
    //masterRow.set("TOTALTAX", totalTax);
    vcl = me;
    Ext.getCmp('BWTOTALAMOUNT0_' + vcl.winId).setValue(bwTotalAmount);
    Ext.getCmp('BWTOTALTAXAMOUNT0_' + vcl.winId).setValue(bwTotalTaxAmount);
    Ext.getCmp('BWTOTALTAX0_' + vcl.winId).setValue(bwTotalTax);
    Ext.getCmp('TOTALAMOUNT0_' + vcl.winId).setValue(totalAmount);
    Ext.getCmp('TOTALTAXAMOUNT0_' + vcl.winId).setValue(totalTaxAmount);
    Ext.getCmp('TOTALTAX0_' + vcl.winId).setValue(totalTax);
    //masterRow.data["BWTOTALAMOUNT"] = bwTotalAmount;
    //masterRow.data["BWTOTALTAXAMOUNT"] = bwTotalTaxAmount;
    //masterRow.data["BWTOTALTAX"] = bwTotalTax;
    //masterRow.data["TOTALAMOUNT"] = totalAmount;
    //masterRow.data["TOTALTAXAMOUNT"] = totalTaxAmount;
    //masterRow.data["TOTALTAX"] = totalTax;
    vcl.forms[0].updateRecord(masterRow);
}

//填充当前行特征信息
function FillDataRow(e, This, CodeDesc) {
    e.dataInfo.dataRow.set("ATTRIBUTECODE", CodeDesc.Code);
    e.dataInfo.dataRow.set("ATTRIBUTEDESC", CodeDesc.Desc);
    return true;
}

//最新特征窗体
function CreatAttForm(dataList, returnData, This, row, method) {
    var MaterialId = dataList.MaterialId;
    var AttributeId = dataList.AttributeId;
    var AttributeName = dataList.AttributeName;
    var AttributeCode = dataList.AttributeCode;
    var BillNo = dataList.BillNo;
    var Row_Id = dataList.Row_Id;
    var standard = [];
    var unstandard = [];
    if (returnData.length == 0) {
        Ext.Msg.alert("提示", '该产品不存在特征或特征无法获取！');
        return;
    }
    for (var i = 0; i < returnData.length; i++) {
        if (returnData[i].Dynamic) {
            if (returnData[i].Standard) {
                unstandard.push(CreatTextBox(returnData[i]));
            }
            else {
                standard.push(CreatTextBox(returnData[i]));
            }
        }
        else {
            if (returnData[i].Standard) {
                unstandard.push(CreatComBox(returnData[i]));
            }
            else {
                standard.push(CreatComBox(returnData[i]));
            }
        }
    }
    //标准特征Panel
    var attPanel = new Ext.form.Panel({

    })
    //非标准特征Panel
    var unattPanel = new Ext.form.Panel({

    })
    //确认按钮
    var btnConfirm = new Ext.Button({
        width: 200,
        height: 35,
        text: "确定",
        type: 'submit',
        handler: function () {
            var yes = true;
            var thisWin = Ext.getCmp("attWin" + BillNo + Row_Id + MaterialId);
            if (This.billAction == BillActionEnum.Modif || This.billAction == BillActionEnum.AddNew) {

                var attPanel = thisWin.items.items[0];
                var unattPanel = thisWin.items.items[1];
                var attributeId = thisWin.attributeId;
                var materialId = thisWin.materialId;
                var attDic = [];
                var msg = '';
                for (var i = 0; i < attPanel.items.length; i++) {
                    if (attPanel.items.items[i].value == null) {
                        msg += '【' + attPanel.items.items[i].fieldLabel + '】';
                    }
                    else {
                        if (attPanel.items.items[i].id.indexOf("numberfield") >= 0 && attPanel.items.items[i].value <= 0) {
                            Ext.Msg.alert("提示", '标准特征项【' + attPanel.items.items[i].fieldLabel + '】的值必须大于0！');
                            return false;
                        }
                        attDic.push({ AttributeId: attPanel.items.items[i].attId, AttrCode: attPanel.items.items[i].value })
                    }
                }
                if (msg.length > 0) {
                    Ext.Msg.alert("提示", '请维护标准特征项中' + msg + '的值！');
                    return false;
                }
                for (var i = 0; i < unattPanel.items.length; i++) {
                    if (unattPanel.items.items[i].value != null) {
                        attDic.push({ AttributeId: unattPanel.items.items[i].attId, AttrCode: unattPanel.items.items[i].value })
                    }
                }
                if (attDic.length > 0) {
                    var CodeDesc = This.invorkBcf('GetAttrInfo', [materialId, attributeId, attDic]);
                    yes = method(row, This, CodeDesc);
                }
            }
            if (yes) {
                thisWin.close();
            }

        }
    })
    //取消按钮
    var btnCancel = new Ext.Button({
        width: 200,
        height: 35,
        text: "取消",
        type: 'submit',
        handler: function () {
            Ext.getCmp("attWin" + BillNo + Row_Id + MaterialId).close();
        }
    })
    //按钮Panle
    var btnPanel = new Ext.form.Panel({
        layout: 'column',
        width: '100%',
        defaults: {
            margin: '10 40 0 40',
            columnWidth: .5
        },
        items: [btnConfirm, btnCancel]
    })

    var win = new Ext.create('Ext.window.Window', {
        id: "attWin" + BillNo + Row_Id + MaterialId,
        title: '特征信息',
        resizable: false,
        //closeAction: "close",
        modal: true,
        width: 600,
        height: 330,
        materialId: MaterialId,
        attributeId: AttributeId,
        autoScroll: true,
        layout: 'column',
        items: [{
            id: 'Att' + attId,
            layout: 'column',
            xtype: 'fieldset',
            title: '标准特征',
            //collapsed: true,
            collapsible: true,
            width: '96%',
            height: 200,
            defaulType: 'combobox',
            margin: '5 10 5 10',
            autoScroll: true,
            items: standard,
            listeners: {
                collapse: function (a, b) {
                    //Ext.getCmp('no'+ a.id).expand();
                },
                expand: function (a, b) {
                    Ext.getCmp('no' + a.id).collapse(true);
                }
            },
        }, {
            id: 'noAtt' + attId,
            layout: 'column',
            xtype: 'fieldset',
            //collapsed: true,
            collapsible: true,
            width: '96%',
            height: 200,
            margin: '5 10 0 10',
            title: '非标准特征',
            autoScroll: true,
            items: unstandard,
            listeners: {
                collapse: function (a, b) {
                    //Ext.getCmp(a.id.substr(2, a.id.length - 2)).expand();
                },
                expand: function (a, b) {
                    Ext.getCmp(a.id.substr(2, a.id.length - 2)).collapse(true);
                }
            }
        }, btnPanel],
    });
    attId++;
    win.show();
    win.items.items[1].collapse(true);
}

//非动态特征 combox
function CreatComBox(attData) {

    var attlist = [];
    for (var i = 0; i < attData.AttrValueList.length; i++) {
        var data = { AttrCode: attData.AttrValueList[i]['AttrCode'], AttrValue: attData.AttrValueList[i]['AttrValue'] };
        attlist.push(data);
    }
    var Store = Ext.create("Ext.data.Store", {
        fields: ["AttrCode", "AttrValue"],
        data: attlist
    });
    var combox = new Ext.form.ComboBox({
        mode: 'local',
        forceSelection: true,
        triggerAction: 'all',
        displayField: 'AttrValue',
        fieldLabel: attData.AttributeItemName,
        attId: attData.AttributeItemId,
        valueField: 'AttrCode',
        fields: ['AttrCode', 'AttrValue'],
        store: Store,
        value: attData.DefaultValue,
        //editable: true,
        //text: attData.DefaultValue,
        margin: '5 10 5 10',
        columnWidth: .5,
        labelWidth: 60,
    });
    return combox;
}

//动态特征 NumberField
function CreatTextBox(attData) {
    var textbox = new Ext.form.NumberField({
        fieldLabel: attData.AttributeItemName,
        attId: attData.AttributeItemId,
        allowDecimals: false, // 允许小数点
        allowNegative: false, // 允许负数
        allowBlank: false,
        value: attData.DefaultValue,
        maxLength: 50,
        margin: '5 10 5 10',
        columnWidth: .5,
        labelWidth: 60,
    });
    return textbox;
}

//重置按钮
function ResetPanel(returnData, e, This) {

    for (var i = 0; i < returnData.length ; i++) {
        var AttributeCode = "";
        var AttributeDesc = "";
        if (returnData[i]["AttributeCode"] != undefined) {
            AttributeCode = returnData[i]["AttributeCode"];
            AttributeDesc = returnData[i]["AttributeDesc"];
        }

        var thisRow = {
            AttributeId: returnData[i]["AttributeId"],
            MaterialId: returnData[i]['MaterialId'],
            MaterialtypeName: returnData[i]['MaterialtypeName'],
            MaterialtypeId: returnData[i]['MaterialtypeId'],
            MaterialName: returnData[i]['MaterialName'],
            BillNo: e.dataInfo.dataRow.data["BILLNO"],
            RowId: e.dataInfo.dataRow.data["ROW_ID"],
            Quantity: returnData[i]['Quantity'],
            IsNotAdd: true,
            AttributeCode: AttributeCode,
            AttributeDesc: AttributeDesc,

        }
        var panel = AddPanel(thisRow, e, This)
        newPanel.add(panel);
    }
}
