import sys, os, string, re, zipfile, time
import xml.etree.ElementTree as ET 
from collections import defaultdict
import threading
import traceback

import xlrd
from portalocker.exceptions import LockException

from form import Ui_MainForm
from addmgr import AddMgr
from settingmgr import SettingMgr
from data import toolData, PASS_KEY_WORDS
from excel2lua_jp import parse_excel as parse_excel_jp
from excel2lua_tw import parse_excel as parse_excel_tw
from excel2lua_cn import parse_excel as parse_excel_cn
from excel2lua_kr import parse_excel as parse_excel_kr
from check_syntax import LuaSyntaxErrorExcetion

from PyQt5 import QtCore, QtGui, QtWidgets

version = "v20201022"

client_template = string.Template("""
local value_list = {}

local mergedatas = {
$contents
}

if DeviceManager.platform == "windows" then
    local temp = {}
    for k, v in ipairs(mergedatas) do
        local data = require(v)
        for kk, vv in pairs(data) do
            if temp[kk] then
                error(kk .. " is conflict between " .. temp[kk] .. " and " .. v .. ", 快叫凯达去改表")
            else
                temp[kk] = v
            end
        end 
    end
end

for k,v in pairs(mergedatas) do
    local data = require(v)
    table.merge(value_list, data)
end

setmetatable(value_list, {__index = function(t, k)
    if k then
        print("$file id is nil: "..k)
    end
    return nil
end})

return value_list
""")
server_template = string.Template("""
local unpack, ptr, len = ...
local path = unpack(ptr, len)

local datapath = path .. "/data/"
local libpath = path .. "/?.lua;"

package.path = libpath .. package.path

local m = require "sharedataconf.mergedata"

local datalist = {
$contents
}

local t = {}

m.mergedata(t, datalist)

return t
""")

export_conf_template = string.Template("""
local data_files = _ENV
local path, confpath = ...

$contents_common
--合表文件
$contents_merge
""")

#生成资源文件目录访问路径
def resource_path(relative_path):
    if getattr(sys, 'frozen', False): #是否Bundle Resource
        base_path = sys._MEIPASS
    else:
        base_path = os.path.abspath(".")
    return os.path.join(base_path, relative_path)

class ParseExcelThread(QtCore.QThread):
    def __init__(self, export_list, region, check_cache, save_cahce):
        super().__init__()
        self.check_cache = check_cache
        self.save_cahce = save_cahce
        self.export_list = export_list
        self.error = None
        if region == "jp":
            self.parse = parse_excel_jp
        elif region == "tw":
            self.parse = parse_excel_tw
        elif region == "cn":
            self.parse = parse_excel_cn
        elif region == "kr":
            self.parse = parse_excel_kr
        
    
    def run(self):
        for record in self.export_list:
            try:
                if not self.check_cache(record[0], record[1], record[2]):
                    self.parse(record)
                    self.save_cahce(record[0], record[1], record[2])
            except LuaSyntaxErrorExcetion as e:
                print(e)
                self.error = e
                break
            except Exception as e:
                print(e)
                self.error = e
                traceback.print_exc()
                break

class ParseMergeTableThread(QtCore.QThread):
    def __init__(self, obj):
        super().__init__()
        self.obj = obj
    
    def run(self):
        self.obj.generate_merge_table()

class MyOutPut(QtCore.QObject):
    
    newLog = QtCore.pyqtSignal(str)

    def __init__(self):
        super().__init__()

    def write(self, message):
        self.newLog.emit(message)



class MainMgr(Ui_MainForm):
    def __init__(self):
        self.output = MyOutPut()
        sys.stdout = self.output
        self.currentTableDatas = []
        self.selectedItems = []
        self.unselectedItems = []
        self.allSelected = False
        self.output.newLog.connect(self.outputLog)
        self.exporting = False
        self.export_thread = None
        self.merge_thread = None
        
    def resource_path(self, relative_path):
        if getattr(sys, 'frozen', False): 
            base_path = sys._MEIPASS
        else:
            base_path = os.path.abspath(".")
        return os.path.join(base_path, relative_path)

    def setupUi(self, MainForm):
        self.form = MainForm
        super().setupUi(MainForm)
        self.form.setWindowTitle("导表工具{}({})".format(version, os.getcwd()))
        self.gifLabel.setVisible(False)
        gif = QtGui.QMovie(resource_path(os.path.join("res","loading.gif")))
        self.gifLabel.setMovie(gif)
        gif.start()

        self.plainTextEdit.setCenterOnScroll(True)

        self.settingButton.clicked.connect(self.onSettingBtnClick)
        self.exportButton.clicked.connect(self.onExportBtnClick)
        self.addButton.clicked.connect(self.onAddBtnClick)
        self.mergeButton.clicked.connect(self.onMergeBtnClick)

        self.searchLine.textChanged.connect(self.doSearch)
        self.form.setWindowFlags(self.form.windowFlags() & ~QtCore.Qt.WindowMaximizeButtonHint)
        self.form.setFixedSize(self.form.width(), self.form.height())

        #设置icon
        filename = self.resource_path(os.path.join("res","bitbug_favicon.ico"))
        icon = QtGui.QIcon()
        icon.addPixmap(QtGui.QPixmap(filename), QtGui.QIcon.Normal, QtGui.QIcon.Off)
        self.form.setWindowIcon(icon)
        #日志输出设置
        self.plainTextEdit.ensureCursorVisible()

    def init_data(self):
        try:
            #数据初始化
            toolData.init()
            toolData.load_data()
            #初始化表格
            self.initTableWidget()
        except PermissionError:
            print("不要重复打开导表工具")
        self.plainTextEdit.moveCursor(QtGui.QTextCursor.Start)

    #初始表
    def initTableWidget(self):
        #设置不可编辑
        self.tableWidget.setEditTriggers(QtWidgets.QAbstractItemView.NoEditTriggers)
        #设置选中行
        self.tableWidget.setSelectionBehavior(QtWidgets.QAbstractItemView.SelectRows)
        #添加表头
        header = self.tableWidget.horizontalHeader()
        header.sectionDoubleClicked.connect(self.headerDoubleClick)
        #设置1、2、3、4 5列表自适应文字
        header.setSectionResizeMode(1, QtWidgets.QHeaderView.ResizeToContents)
        header.setSectionResizeMode(2, QtWidgets.QHeaderView.ResizeToContents)
        header.setSectionResizeMode(3, QtWidgets.QHeaderView.ResizeToContents)
        header.setSectionResizeMode(4, QtWidgets.QHeaderView.ResizeToContents)
        header.setSectionResizeMode(5, QtWidgets.QHeaderView.ResizeToContents)
        files = toolData.get_files()
        self.reloadTable()

        #注册配置变动事件
        toolData.fileParentUpdateEvent.connect(self.parent_update_event)
        
        #注册右键事件
        self.tableWidget.setContextMenuPolicy(QtCore.Qt.CustomContextMenu)
        self.tableWidget.customContextMenuRequested.connect(self.openRightMenu)

        #双击事件
        self.tableWidget.doubleClicked.connect(self.tableDoubleClick)
        self.tableWidget.cellClicked.connect(self.cellClicked)

    def parent_update_event(self):
        self.reloadTable()

    def getCheckBox(self, row):
        return self.tableWidget.item(row, 0)

    def headerDoubleClick(self, logical):
        if logical == 0:
            if self.allSelected:
                self.selectedItems = []
            else:
                for item in self.currentTableDatas:
                    if item not in self.selectedItems:
                        self.selectedItems.append(item)
            self.allSelected = not self.allSelected
            self.reloadTable()

    def changeItemCheck(self, row):
        selectedCount = len(self.selectedItems)
        if row < selectedCount:
            self.selectedItems.pop(row)
        else:
            file = self.unselectedItems[row-selectedCount]
            self.selectedItems.append(file)
        self.reloadTable()
    
    def tableDoubleClick(self, modelIdx):
        row = modelIdx.row()
        self.changeItemCheck(row)
        # item = self.getCheckBox(row)
        # st = item.checkState()
        # if st == QtCore.Qt.Unchecked:
        #     st = QtCore.Qt.Checked
        # else:
        #     st = QtCore.Qt.Unchecked
        # item.setCheckState(st)

    def cellClicked(self, row, col):
        if col == 0:
            self.changeItemCheck(row)
            # item = self.getCheckBox(row)
            # st = item.checkState()
            # if st == QtCore.Qt.Unchecked:
            #     st = QtCore.Qt.Checked
            # else:
            #     st = QtCore.Qt.Unchecked
            # item.setCheckState(st)


    def openRightMenu(self, position):
        modelIdx = self.tableWidget.indexAt(position)
        row = modelIdx.row()
        if row < 0:
            return
        menu = QtWidgets.QMenu()
        modifyAction = menu.addAction("编辑")
        delAction = menu.addAction("删除")
        copyAction = menu.addAction("拷贝")
        path = self.tableWidget.item(row, 1).text()
        sheet = self.tableWidget.item(row, 2).text()
        to = self.tableWidget.item(row, 3).text()
        parents = self.tableWidget.item(row, 4).text()
        encrypt = self.tableWidget.item(row, 5).text()

        action = menu.exec_(self.tableWidget.mapToGlobal(QtCore.QPoint(position.x()+50, position.y()+20)))
        if action == delAction:
            ret = QtWidgets.QMessageBox.question(self.form, "警告", "确定删除 [%s]>[%s] ?"%(path, sheet))
            if ret == QtWidgets.QMessageBox.No:
                return
            if toolData.remove_file(path, sheet, to):
                if row < len(self.selectedItems):
                    self.selectedItems.pop(row)
                self.reloadTable()
        elif action == modifyAction:
            AddDialog = QtWidgets.QDialog()
            ui = AddMgr("modify", {"path":path, "sheet":sheet, "to":to, "parents":parents.split(","), "encrypt":encrypt})
            ui.setupUi(AddDialog)
            code = AddDialog.exec_()
            if code == 1:
                self.reloadTable()
        elif action == copyAction:
            AddDialog = QtWidgets.QDialog()
            ui = AddMgr('add', {'path':path,  'sheet':sheet,  'to':to,  'parents':parents.split(','), "encrypt":encrypt})
            ui.setupUi(AddDialog)
            code = AddDialog.exec_()
            if code == 1:
                self.reloadTable()
    
    def renderItem(self, file, idx, checked=False):
        i = idx
        check = self.getCheckBox(i)
        if not check:
            check = QtWidgets.QTableWidgetItem()
            self.tableWidget.setItem(i, 0, check)
        check.setFlags(QtCore.Qt.ItemIsUserCheckable | check.flags())
        st = checked and QtCore.Qt.Checked or QtCore.Qt.Unchecked
        check.setCheckState(st)
        path = self.tableWidget.item(i, 1)
        if not path:
            path = QtWidgets.QTableWidgetItem()
            self.tableWidget.setItem(i, 1, path)
        path.setText(file["path"])
        sheet = self.tableWidget.item(i, 2)
        if not sheet:
            sheet = QtWidgets.QTableWidgetItem()
            self.tableWidget.setItem(i, 2, sheet)
        sheet.setText(file["sheet"])
        to = self.tableWidget.item(i, 3)
        if not to:
            to = QtWidgets.QTableWidgetItem()
            self.tableWidget.setItem(i, 3, to)
        to.setText(file["to"])
        parents = self.tableWidget.item(i, 4)
        if not parents:
            parents = QtWidgets.QTableWidgetItem()
            self.tableWidget.setItem(i, 4, parents)
        parents.setText(','.join(file["parents"]))
        encrypt = self.tableWidget.item(i, 5)
        if not encrypt:
            encrypt = QtWidgets.QTableWidgetItem()
            self.tableWidget.setItem(i, 5, encrypt)
        encrypt.setText(file.get("encrypt", ""))

    def reloadTable(self):
        search_text = self.searchLine.text()
        files = toolData.get_files()
        if search_text:
            self.currentTableDatas = []
            search_text = search_text.upper()
            for f in files:
                if search_text in f["path"].upper() or search_text in f["sheet"].upper() or search_text in f["to"].upper():
                    self.currentTableDatas.append(f)
                else:
                    for p in f["parents"]:
                        if search_text in p.upper():
                            self.currentTableDatas.append(f)
        else:
            self.currentTableDatas = files.copy()
        self.tableWidget.setRowCount(0)
        self.tableWidget.setRowCount(len(files))
        idx = 0
        for i in range(len(self.selectedItems)):
            file = self.selectedItems[i]
            self.renderItem(file, idx, True)
            idx = idx + 1
        self.unselectedItems = []
        for i in range(len(self.currentTableDatas)):
            file = self.currentTableDatas[i]
            if file not in self.selectedItems:
                self.renderItem(file, idx)
                idx = idx + 1
                self.unselectedItems.append(file)
        self.tableWidget.setRowCount(idx)
    
    def onSettingBtnClick(self):
        dlg = QtWidgets.QDialog()
        ui = SettingMgr()
        ui.setupUi(dlg)
        code = dlg.exec_()

    def onAddBtnClick(self):
        AddDialog = QtWidgets.QDialog()
        ui = AddMgr()
        ui.setupUi(AddDialog)
        code = AddDialog.exec_()
        if code == 1:
            files = toolData.get_files()
            self.reloadTable()

    def onExportBtnClick(self):
        if self.exporting: return
        self.gifLabel.setVisible(True)
        self.exportButton.setEnabled(False)
        self.plainTextEdit.clear()
        root = toolData.get_rootpath()
        excelpath = os.path.join(root, "Excel")
        exportpath = os.path.join(root, "Excel", "Temp")
        exportjson = os.path.join(root, "Assets", "Neon", "Datas")
        export_list = []
        for item in self.selectedItems:
            path = os.path.join(excelpath, item["path"] + '.xlsx')
            sheet = item["sheet"]
            to = os.path.join(exportpath, item["to"] + '.lua')
            encrypt = "encrypt" in item and item["encrypt"] and os.path.join(exportpath, item["encrypt"] + '.lua') or None
            tojson = os.path.join(exportjson, item["to"] + ".json")
            if sheet not in PASS_KEY_WORDS:
                export_list.append([path, sheet, to, encrypt, tojson])
        if len(export_list):
            self.export_thread = ParseExcelThread(export_list, toolData.get_value("region"), toolData.is_cached, toolData.save_cached)
            self.export_thread.finished.connect(self.parse_thread_finish)
            self.export_thread.start()

        else:
            self.exportButton.setEnabled(True)
            self.gifLabel.setVisible(False)
    
    def parse_thread_finish(self):
        self.gifLabel.setVisible(False)
        self.exportButton.setEnabled(True)
        if self.export_thread.error:
            QtWidgets.QMessageBox.critical(self.form, "错误", "导表终止，查看错误日志")
            print("导表终止: ", self.export_thread.error)
        else:
            print("导表完成")
    
    def onMergeBtnClick(self):
        self.mergeButton.setEnabled(False)
        self.gifLabel.setVisible(True)
        self.merge_thread = ParseMergeTableThread(self)
        self.merge_thread.finished.connect(self.merge_thread_finish)
        self.merge_thread.start()
    
    def merge_thread_finish(self):
        self.gifLabel.setVisible(False)
        self.mergeButton.setEnabled(True)
    
    tag1 = "{http://schemas.openxmlformats.org/officeDocument/2006/extended-properties}TitlesOfParts"
    tag2 = "{http://schemas.openxmlformats.org/officeDocument/2006/docPropsVTypes}vector"
    tag3 = "{http://schemas.openxmlformats.org/officeDocument/2006/docPropsVTypes}lpstr"
    x_sheet_name_path = "%s/%s/%s"%(tag1, tag2, tag3)
    def check_xlsx_exists(self, path, sheet):
        if path in PASS_KEY_WORDS:
            return True
        try:
            path = os.path.join(toolData.get_rootpath(), "导表excel", path + ".xlsx")
            with zipfile.ZipFile(path) as zf:
                with zf.open("docProps/app.xml") as fd:
                    root = ET.parse(fd).getroot()
                    # print(root.tag, root.text, root.attrib)
                    sheets = root.findall(self.x_sheet_name_path)
                    for s in sheets:
                        # print(s.text, sheet)
                        if s.text == sheet:
                            return True
        except Exception as e:
            print("[ERROR]", path, e)
            return False
        return False
    
    def check_export_file(self, to):
        src = "src"
        region = toolData.get_value("region")
        if region == "jp":
            src = "srcjp"
        elif region == "tw":
            src = "srctw"
        elif region == "kr":
            src = "src_kr"
        path = os.path.join(toolData.get_rootpath(), src, "data", to+".lua")
        return os.path.exists(path)

    def generate_merge_table(self):
        merge_map = defaultdict(list)
        confs_list_common = []
        confs_list_parents = []
        files = toolData.get_files()
        plist = toolData.get_value("server_exclude_path") or []
        pattern = '|'.join(plist)
        merge_path_exclude = toolData.get_value("server_merge_exclude_path") or []
        for f in files:
            if not self.check_xlsx_exists(f['path'], f['sheet']) and not self.check_export_file(f['to']):
                print("【错误】[%s]->[%s]=>[%s] 文件不存在, 并且旧的导出文件也不存在 跳过合表！！！"%(f['path'], f['sheet'], f['to']))
                continue
            for p in f["parents"]:
                l = merge_map[p]
                l.append(f["to"])
            if not f["parents"]:
                subs = f["to"].split("\\")
                if not re.search(pattern, f["to"]):
                    confs_list_common.append('{0:<50} = path .. "{1}.lua"'.format("data_files[\""+subs[-1]+"\"]", "/".join(subs)))
        root = toolData.get_rootpath()
        clientpath = os.path.join(root, "src", "data")
        
        serverpath = os.path.join(root, "sharedataconf")
        client_extra = toolData.get_value("region")
        if client_extra == "jp":
            client_extra = os.path.join(root, "srcjp", "data")
        elif client_extra == "tw":
            client_extra = os.path.join(root, "srctw", "data")
        elif client_extra == "kr":
            client_extra = os.path.join(root, "src_kr", "data")
        elif client_extra == "cn":
            client_extra = os.path.join(root, "server2", "data")
        else:
            client_extra = None
        
        for p, sub in merge_map.items():
            parent_path_list = p.replace("\\\\", "/").replace("\\", "/").split("/")
            client_list = []
            server_list = []
            sub.sort()
            for s in sub:
                sub_path = s.split("\\")
                client_list.append('    "data.%s",'%('.'.join(sub_path)))
                server_list.append('    datapath.."%s.lua",'%('/'.join(sub_path)))
            #export client
            client_list.sort()
            server_list.sort()
            path = os.path.join(clientpath, *parent_path_list) + ".lua"
            content = client_template.substitute(contents='\n'.join(client_list), file=parent_path_list[-1])
            os.makedirs(os.path.dirname(path), exist_ok=True)
            with open(path, "w", encoding="utf8") as fd:
                fd.write(content)
            if client_extra:
                path = os.path.join(client_extra, *parent_path_list) + ".lua"
                os.makedirs(os.path.dirname(path), exist_ok=True)
                with open(path, "w", encoding="utf8") as fd:
                    fd.write(content)
            #export server
            content = server_template.substitute(contents='\n'.join(server_list))
            path = os.path.join(serverpath, parent_path_list[-1]) + ".lua"
            os.makedirs(os.path.dirname(path), exist_ok=True)
            with open(path, "w", encoding="utf8") as fd:
                fd.write(content)
            #add to server conf
            if parent_path_list[-1] not in merge_path_exclude:
                confs_list_parents.append('{0:<50} = confpath .. "{1}.lua"'.format('data_files["'+parent_path_list[-1]+'"]', parent_path_list[-1]))
        #export server conf
        os.makedirs(serverpath, exist_ok=True)
        confs_list_common.sort()
        confs_list_parents.sort()
        with open(os.path.join(serverpath, "conf_export.lua"), "w", encoding="utf8") as fd:
            contents = export_conf_template.substitute(contents_common="\n".join(confs_list_common), contents_merge="\n".join(confs_list_parents))
            fd.write(contents)
        print("合表导出完成！")

    def doSearch(self, text):
        self.reloadTable()
    
    def outputLog(self, msg):
        self.plainTextEdit.insertPlainText(msg)
        self.plainTextEdit.ensureCursorVisible()

if __name__ == "__main__":
    import sys
    app = QtWidgets.QApplication(sys.argv)
    MainForm = QtWidgets.QWidget()
    ui = MainMgr()
    ui.setupUi(MainForm)
    ui.init_data()
    MainForm.show()
    sys.exit(app.exec_())
