# -*- coding: utf-8 -*-

# Form implementation generated from reading ui file 'ui\main.ui'
#
# Created by: PyQt5 UI code generator 5.15.0
#
# WARNING: Any manual changes made to this file will be lost when pyuic5 is
# run again.  Do not edit this file unless you know what you are doing.


from PyQt5 import QtCore, QtGui, QtWidgets


class Ui_MainForm(object):
    def setupUi(self, MainForm):
        MainForm.setObjectName("MainForm")
        MainForm.resize(1271, 874)
        self.settingButton = QtWidgets.QPushButton(MainForm)
        self.settingButton.setGeometry(QtCore.QRect(570, 20, 71, 40))
        self.settingButton.setObjectName("settingButton")
        self.tableWidget = QtWidgets.QTableWidget(MainForm)
        self.tableWidget.setGeometry(QtCore.QRect(10, 80, 1251, 611))
        self.tableWidget.setObjectName("tableWidget")
        self.tableWidget.setColumnCount(6)
        self.tableWidget.setRowCount(0)
        item = QtWidgets.QTableWidgetItem()
        self.tableWidget.setHorizontalHeaderItem(0, item)
        item = QtWidgets.QTableWidgetItem()
        self.tableWidget.setHorizontalHeaderItem(1, item)
        item = QtWidgets.QTableWidgetItem()
        self.tableWidget.setHorizontalHeaderItem(2, item)
        item = QtWidgets.QTableWidgetItem()
        self.tableWidget.setHorizontalHeaderItem(3, item)
        item = QtWidgets.QTableWidgetItem()
        self.tableWidget.setHorizontalHeaderItem(4, item)
        item = QtWidgets.QTableWidgetItem()
        self.tableWidget.setHorizontalHeaderItem(5, item)
        self.searchLine = QtWidgets.QLineEdit(MainForm)
        self.searchLine.setGeometry(QtCore.QRect(9, 20, 551, 40))
        self.searchLine.setInputMask("")
        self.searchLine.setText("")
        self.searchLine.setObjectName("searchLine")
        self.exportButton = QtWidgets.QPushButton(MainForm)
        self.exportButton.setGeometry(QtCore.QRect(850, 20, 91, 40))
        self.exportButton.setObjectName("exportButton")
        self.addButton = QtWidgets.QPushButton(MainForm)
        self.addButton.setGeometry(QtCore.QRect(660, 20, 70, 40))
        self.addButton.setObjectName("addButton")
        self.label = QtWidgets.QLabel(MainForm)
        self.label.setGeometry(QtCore.QRect(10, 700, 54, 12))
        self.label.setObjectName("label")
        self.plainTextEdit = QtWidgets.QPlainTextEdit(MainForm)
        self.plainTextEdit.setGeometry(QtCore.QRect(10, 720, 1251, 141))
        self.plainTextEdit.setReadOnly(True)
        self.plainTextEdit.setObjectName("plainTextEdit")
        self.gifLabel = QtWidgets.QLabel(MainForm)
        self.gifLabel.setGeometry(QtCore.QRect(1200, 20, 51, 41))
        self.gifLabel.setObjectName("gifLabel")
        self.mergeButton = QtWidgets.QPushButton(MainForm)
        self.mergeButton.setGeometry(QtCore.QRect(750, 20, 75, 41))
        self.mergeButton.setObjectName("mergeButton")

        self.retranslateUi(MainForm)
        QtCore.QMetaObject.connectSlotsByName(MainForm)

    def retranslateUi(self, MainForm):
        _translate = QtCore.QCoreApplication.translate
        MainForm.setWindowTitle(_translate("MainForm", "导表"))
        self.settingButton.setText(_translate("MainForm", "设置"))
        item = self.tableWidget.horizontalHeaderItem(0)
        item.setText(_translate("MainForm", "双击"))
        item = self.tableWidget.horizontalHeaderItem(1)
        item.setText(_translate("MainForm", "路径"))
        item = self.tableWidget.horizontalHeaderItem(2)
        item.setText(_translate("MainForm", "sheet"))
        item = self.tableWidget.horizontalHeaderItem(3)
        item.setText(_translate("MainForm", "导出"))
        item = self.tableWidget.horizontalHeaderItem(4)
        item.setText(_translate("MainForm", "合表"))
        item = self.tableWidget.horizontalHeaderItem(5)
        item.setText(_translate("MainForm", "混淆"))
        self.searchLine.setPlaceholderText(_translate("MainForm", "搜索"))
        self.exportButton.setText(_translate("MainForm", "数据导出"))
        self.addButton.setText(_translate("MainForm", "添加"))
        self.label.setText(_translate("MainForm", "输出"))
        self.gifLabel.setText(_translate("MainForm", "12"))
        self.mergeButton.setText(_translate("MainForm", "合表导出"))


if __name__ == "__main__":
    import sys
    app = QtWidgets.QApplication(sys.argv)
    MainForm = QtWidgets.QWidget()
    ui = Ui_MainForm()
    ui.setupUi(MainForm)
    MainForm.show()
    sys.exit(app.exec_())
