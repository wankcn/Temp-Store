import os, sys
import json
from operator import itemgetter
from PyQt5.QtCore import QObject, pyqtSignal
import portalocker

"""
{
    "files": [
        {
            "path": "../../a.xlsx",
            "sheet": "sheet1",
            "to": "../a.lua",
            "parents":[
                "item_data.lua",
                "weapon_data.lua"
            ]
        }
    ],
    "parents": [
        "item_data.lua",
        "level_data.lua",
        "levelmode_data.lua",
    ],
    "rootpath": "../"
}


"""

PASS_KEY_WORDS = ('missing', 'merge', 'pass')

FILE_FIELDS = sorted(["parents", "path", "sheet", "to", "encrypt"])
FILE_FIELDS2 = sorted(["parents", "path", "sheet", "to"])

class ToolData(QObject):
    _instance = None
    filepath = "exportdata.json"
    cache_file = "cache.dat"
    fileParentUpdateEvent = pyqtSignal()
    def __new__(cls):
        if cls._instance is None:
            cls._instance = super(ToolData, cls).__new__(cls)
            # Put any initialization here.
            cls._instance.__init__()
        return cls._instance

    def __init__(self):
        # Don`t do any init here
        super().__init__()
        self.cache = []


    def init(self):
        self.data_fd = open(self.filepath, "r+", encoding="utf8")
        self.data_fd.read()
        portalocker.lock(self.data_fd, portalocker.LOCK_EX)

    def __check_data_valid(self):
        to_list = []
        path_list = []
        valid = True
        for file in self.data["files"]:
            sk = sorted(list(file.keys()))
            if sk!=FILE_FIELDS and sk!=FILE_FIELDS2:
                print("字段缺失", file)
                valid = False
            else:
                to = file.get("to")
                if to:
                    if to in to_list:
                        print("导出重复", file)
                        valid = False
                    else:
                        to_list.append(to)
                path = file["path"]
                sheet = file["sheet"]
                if path!=sheet or path not in PASS_KEY_WORDS:
                    k = path + sheet
                    if k in path_list:
                        print("文件路径重复", file)
                        valid = False
                    else:
                        path_list.append(k)
        return valid

    # load data from store file
    def load_data(self):
        if os.path.exists(self.filepath) and os.stat(self.filepath).st_size:
            try:
                self.data_fd.seek(0)
                self.data = json.load(self.data_fd)
                valid = self.__check_data_valid()
                if not valid:
                    raise Exception("data invalid")
            except Exception as e:
                print("数据加载出错", e)
                self.data = {
                    'files': [],
                    'parents': []
                }
        else:
            self.data = {
                'files': [],
                'parents': []
            }
        if os.path.exists(self.cache_file):
            with open(self.cache_file, "r", encoding="utf8") as fd:
                tmp = fd.read()
                self.cache = tmp.split("\n")
        else:
            self.cache = None

    
    def __save(self):
        self.data["files"].sort(key=itemgetter("to", "path", "sheet"))
        dumpdata = self.data.copy()
        dumpdata["files"] = 20200826
        file_dumps = []
        for file in self.data["files"]:
            file_dumps.append(json.dumps(file, sort_keys=True, ensure_ascii=False))
        tstr = json.dumps(dumpdata, sort_keys=True, indent=4, ensure_ascii=False)
        fstr = "[\n        {}\n    ]".format(",\n        ".join(file_dumps))
        dumpstr = tstr.replace('20200826', fstr)
        # fd.write(json.dumps(self.data, sort_keys=True, indent=4, ensure_ascii=False))
        self.data_fd.seek(0)
        self.data_fd.truncate()
        self.data_fd.write(dumpstr)
        self.data_fd.flush()

    def __check_exists(self, path, sheet, to):
        for f in self.data["files"]:
            if f["path"] == path and f["sheet"]==sheet and f["to"]==to:
                return f

    def __check_exists_output(self, to):
        for f in self.data["files"]:
            if f["to"] == to:
                return f

    def add_file(self, path, sheet, to, parents=[], encrypt=""):
        path = path.strip()
        sheet = sheet.strip()
        to = to.strip()
        parents = [x.strip() for x in parents]
        parents.sort()
        encrypt = encrypt.strip()
        if not path or not sheet or not to:
            return False, "参数错误"
        e = self.__check_exists(path, sheet, to)
        if e and not (path==sheet and path in PASS_KEY_WORDS):
            return False, "不能重复添加"
        if self.__check_exists_output(to):
            return False, "导出路径重复"
        if to in self.data["parents"]:
            return False, "和合表路径重复"
        obj = {
            'path': path,
            'sheet': sheet,
            'to': to,
            'parents': parents
        }
        if encrypt:
            obj["encrypt"] = encrypt
        self.data["files"].append(obj)
        self.__save()
        return True, ""
    
    def remove_file(self, path, sheet, to):
        # print(path, sheet, to)
        path = path.strip()
        sheet = sheet.strip()
        to = to.strip()
        e = self.__check_exists(path, sheet, to)
        if e:
            self.data["files"].remove(e)
            self.__save()
            return True
    
    def modify_file(self, old_path, old_sheet, old_to, path, sheet, to, parents=[], encrypt=""):
        old_path = old_path.strip()
        old_sheet = old_sheet.strip()
        old_to = old_to.strip()
        path = path.strip()
        sheet = sheet.strip()
        to = to.strip()
        parents = [x.strip() for x in parents]
        encrypt = encrypt.strip()
        if not path or not sheet or not to:
            return False, "参数错误"
        old = None
        for f in self.data["files"]:
            if f["path"] == old_path and f["sheet"]==old_sheet and f["to"]==old_to:
                old = f
                break
        if not old:
            return False, "not found"
        old["path"] = path
        old["sheet"] = sheet
        old["to"] = to
        old["parents"] = parents
        if encrypt:
            old["encrypt"] = encrypt
        else:
            if "encrypt" in old:
                old["encrypt"] = encrypt
        self.__save()
        return True, ""

    def add_parent(self, p):
        p = p.strip()
        parents = self.data["parents"]
        for f in parents:
            if f == p:
                return False
        parents.append(p)
        parents.sort()
        self.__save()
        return True
    
    #@change 是否同步删除已配置的合表
    def remove_parent(self, p, sync):
        p = p.strip()
        parents = self.data["parents"]
        assert(p in parents)
        parents.remove(p)
        if sync:
            for f in self.data["files"]:
                if p in f["parents"]:
                    f["parents"].remove(p)
            self.fileParentUpdateEvent.emit()
        self.__save()
    

    def modify_parent(self, o, n, sync):
        parents = self.data["parents"]
        idx = parents.index(o)
        parents[idx] = n
        if sync:
            for f in self.data["files"]:
                if o in f["parents"]:
                    idx = f["parents"].index(o)
                    f["parents"][idx] = n
            self.fileParentUpdateEvent.emit()
        self.__save()
    
    def update_rootpath(self, path):
        path = path.strip()
        self.data["rootpath"] = path
        self.__save()
    
    def get_rootpath(self):
        return self.data["rootpath"]

    def get_files(self):
        return self.data["files"]

    def get_parents(self):
        return self.data["parents"]
    
    def get_value(self, key):
        return self.data.get(key, None)

    def is_cached(self, path, sheet, target):
        if self.cache is None: return
        s = path + "->" + sheet + "->" + target
        return s in self.cache
    
    def save_cached(self, path, sheet, target):
        if self.cache is None: return
        s = path + "->" + sheet + "->" + target
        self.cache.append(s)
        with open(self.cache_file, "a", encoding="utf8") as fd:
            fd.write(s + "\n")

try:
    assert(toolData)
except NameError as e:
    toolData = ToolData()

if __name__ == "__main__":
    a = ToolData()