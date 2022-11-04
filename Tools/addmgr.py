from addui import Ui_AddDialog

from PyQt5 import QtCore, QtGui, QtWidgets
from data import toolData

class AddMgr(Ui_AddDialog):
    def __init__(self, tp="add", item=None):
        self.dlg_tp = tp
        self.init_item = item

    def setupUi(self, AddDialog):
        self.dlg = AddDialog
        super().setupUi(AddDialog)
        self.addPushButton.clicked.connect(self.onAddBtnClick)
        self.exitPushButton.clicked.connect(self.onExitBtnClick)
        self.initParentList()
        if self.dlg_tp == "modify":
            self.dlg.setWindowTitle("修改")
        if self.init_item:
            self.excelPathLineEdit.setText(self.init_item["path"])
            self.exportSheetLineEdit.setText(self.init_item["sheet"])
            self.exportPathLineEdit.setText(self.init_item["to"])
            self.exportEncryptLineEdit.setText(self.init_item.get("encrypt", ""))
            if self.init_item["parents"]:
                for i in range(self.parentsListWidget.count()):
                    item = self.parentsListWidget.item(i)
                    if item.text() in self.init_item["parents"]:
                        item.setCheckState(QtCore.Qt.Checked)

    #合表选择列表
    def initParentList(self):
        parents = toolData.get_parents()
        for i in range(len(parents)):
            item = QtWidgets.QListWidgetItem(parents[i])
            item.setFlags(QtCore.Qt.ItemIsUserCheckable | QtCore.Qt.ItemIsSelectable |item.flags())
            item.setCheckState(QtCore.Qt.Unchecked)
            self.parentsListWidget.addItem(item)
        self.parentsListWidget.doubleClicked.connect(self.parentDoubleClicked)
        self.parentsListWidget.itemChanged.connect(self.itemChanged)
    
    def onAddBtnClick(self):
        path = self.excelPathLineEdit.text().strip()
        sheet = self.exportSheetLineEdit.text().strip()
        to = self.exportPathLineEdit.text().strip()
        encrypt = self.exportEncryptLineEdit.text().strip()
        parents = []
        s = self.selectedParentLabel.text()
        if s:
            parents = s.split(",")
        if not path:
            QtWidgets.QMessageBox.critical(self.dlg, "错误", "文件路径不能为空")
            return
        if not sheet:
            QtWidgets.QMessageBox.critical(self.dlg, "错误", "sheet不能为空")
            return
        if not to:
            QtWidgets.QMessageBox.critical(self.dlg, "错误", "导出路径不能为空")
            return
        if self.dlg_tp == "add":
            ok, err = toolData.add_file(path, sheet, to, parents, encrypt)
        else:
            ok, err = toolData.modify_file(self.init_item["path"], self.init_item["sheet"], self.init_item["to"], path, sheet, to, parents, encrypt)
        if not ok:
            QtWidgets.QMessageBox.critical(self.dlg, "错误", err)
            return
        self.dlg.accept()

    def onExitBtnClick(self):
        self.dlg.reject()
    
    def itemCheckStateChange():
        pass
    
    def itemChanged(self, item):
        # print(item)
        tx = item.text()
        st = item.checkState()
        old = self.selectedParentLabel.text()
        l = []
        if old:
            l = old.split(",")
        if st != QtCore.Qt.Unchecked and tx not in l:
            l.append(tx)
        else:
            if tx in l:
                l.remove(tx)
        self.selectedParentLabel.setText(','.join(l))

    def parentDoubleClicked(self, modelIndex):
        item = self.parentsListWidget.item(modelIndex.row())
        st = item.checkState()
        if st == QtCore.Qt.Unchecked:
            st = QtCore.Qt.Checked
        else:
            st = QtCore.Qt.Unchecked
        item.setCheckState(st)

if __name__ == "__main__":
    import sys
    app = QtWidgets.QApplication(sys.argv)
    AddDialog = QtWidgets.QDialog()
    ui = AddMgr()
    ui.setupUi(AddDialog)
    AddDialog.show()
    sys.exit(app.exec_())