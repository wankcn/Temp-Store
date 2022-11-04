from PyQt5 import QtCore, QtGui, QtWidgets
from settingui import Ui_SettingDialog
from data import toolData
from pprint import pprint


class SettingMgr(Ui_SettingDialog):
    def __init__(self):
        self.editing = None

    def setupUi(self, SettingDialog):
        self.dlg = SettingDialog
        super().setupUi(SettingDialog)
        self.addButton.clicked.connect(self.addNewItem)
        self.exitButton.clicked.connect(self.onExitBtnClick)
        self.initParentList()

    #合表管理
    def initParentList(self):
        parents = toolData.get_parents()
        for i in range(len(parents)):
            item = QtWidgets.QListWidgetItem(parents[i])
            item.setFlags(QtCore.Qt.ItemIsEditable | item.flags())
            self.mergedTableListWidget.addItem(item)
        self.mergedTableListWidget.doubleClicked.connect(self.mergedTableDoubleClicked)
        self.mergedTableListWidget.itemDelegate().closeEditor.connect(self.editFinish)

        #注册右键事件
        self.mergedTableListWidget.setContextMenuPolicy(QtCore.Qt.CustomContextMenu)
        self.mergedTableListWidget.customContextMenuRequested.connect(self.openMenu)

    def openMenu(self, position):
        modelIdx = self.mergedTableListWidget.indexAt(position)
        row = modelIdx.row()
        if row < 0: return
        menu = QtWidgets.QMenu()
        delAction = menu.addAction("删除")
        action = menu.exec_(self.mergedTableListWidget.mapToGlobal(position))
        if action == delAction:
            item = self.mergedTableListWidget.item(row)
            ret = QtWidgets.QMessageBox.question(self.dlg, "警告", "确定删除%s?"%item.text())
            if ret == QtWidgets.QMessageBox.No:
                return
            ret = QtWidgets.QMessageBox.question(self.dlg, "提醒", "是否修改已配置该合表的导表?")
            toolData.remove_parent(item.text(), ret==QtWidgets.QMessageBox.Yes)
            self.mergedTableListWidget.takeItem(row)
            

    def addNewItem(self):
        item = QtWidgets.QListWidgetItem()
        item.setFlags(QtCore.Qt.ItemIsEditable | item.flags())
        self.mergedTableListWidget.addItem(item)
        self.mergedTableListWidget.setCurrentItem(item)
        self.mergedTableListWidget.editItem(item)
        self.editing = {
            "item": item,
            "new": True,
            "otext": item.text(),
            "row": self.mergedTableListWidget.row(item)
        }

    def editFinish(self, editor, hint):
        # print(dir(editor))
        #print("editFinish", editor.x(), editor.y(), hint)
        assert(self.editing)
        item = self.editing["item"]
        text = item.text().strip()
        if self.editing["new"]:
            if not text:
                self.mergedTableListWidget.takeItem(self.editing["row"])
                return
            toolData.add_parent(text)
        else:
            text = item.text().strip()
            otext = self.editing["otext"]
            if not text:
                item.setText(otext)
            else:
                if text != otext:
                    ret = QtWidgets.QMessageBox.question(self.dlg, "提醒", "是否修改已配置该合表的导表?")
                    toolData.modify_parent(otext, text, ret==QtWidgets.QMessageBox.Yes)

    def mergedTableDoubleClicked(self, modelIdx):
        item = self.mergedTableListWidget.item(modelIdx.row())
        self.editing = {
            "item": item,
            "new": False,
            "otext": item.text(),
            "row": modelIdx.row(),
        }
    
    def onExitBtnClick(self):
        self.dlg.accept()

if __name__ == "__main__":
    import sys
    app = QtWidgets.QApplication(sys.argv)
    SettingDialog = QtWidgets.QDialog()
    ui = SettingMgr()
    ui.setupUi(SettingDialog)
    SettingDialog.show()
    sys.exit(app.exec_())