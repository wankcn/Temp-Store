import os, json, re
from collections import defaultdict

rename_merge_table = {
    "itemdata": "item_data",
    "leveldata": "level_data",
    "dropdata": "drop_data",
    "modeldata": "model_data",
    "taskdata": "task_data",
    "citydata": "city_data",
}

#检查导表路径
def check_export_path():
    for root, dirs, files in os.walk('E:\\Horcrux_Game\\L2_jp_trunk\\src\\data'):
        for sub in dirs:
            for subroot, d, ff in os.walk(os.path.join(root, sub)):
                assert(not d)
                print(subroot)
#构建合表
def get_parents():
    file2parents = defaultdict(list)
    for root, _, files in os.walk('E:\\Horcrux_Server\\Japan\\branches\\l2server_jp_summer\\common\\lua_for_server\\sharedataconf'):
        for file in files:
            with open(os.path.join(root, file), encoding="utf8") as fd:
                p = os.path.splitext(file)[0]
                for line in fd.readlines():
                    ret = re.match(r'^\s*datapath.*?"(.*).lua"', line)
                    if ret:
                        file2parents[ret.groups(0)[0].replace("/", "\\")].append(p)
    print (json.dumps(file2parents, indent=4, sort_keys=True, ensure_ascii=False))
    return file2parents

#获取所有导表配置
'''
{
    filename : {
        sheet: export_path
    }
}
'''
def get_all_export_conf():
    all_conf = defaultdict(dict)
    all_exports_files = set()
    for root, _, files in os.walk('E:\\Horcrux_Game\\L2_jp_trunk\\python导表器'):
        for fname in files:
            if fname!="readme.txt" and os.path.splitext(fname)[1] == ".txt":
                with open(os.path.join(root, fname), encoding="utf8") as fd:
                    try:
                        contents = fd.read()
                        contents.replace("#", "")
                        obj = eval(contents)
                        for line in obj:
                            print(line[0], line[2])
                            path = os.path.join(*line[0].split("\\")[2:])
                            sheet = line[1]
                            to = os.path.join(*line[2].split("\\")[3:])
                            sheets = all_conf[path]
                            t = sheets.get(sheet)
                            if t:
                                if t != to:
                                    print(fname, line)
                                    raise Exception(123)
                            else:
                                sheets[sheet] = to
                                all_exports_files.add(to)
                    except Exception as e:
                        print("ERROR", fname, e)
    return all_conf

def func():
    all_conf = get_all_export_conf()
    file2parents = get_parents()
    files = []
    for fname, sheets in all_conf.items():
        for sheet, to in sheets.items():
            obj = {
                "parents": file2parents.get(to, {}),
                "path": fname,
                "sheet": sheet,
                "to": to
            }
            files.append(obj)
        with open("src/tmp_struct.txt", "w", encoding="utf8") as fd:
            fd.write(json.dumps(files, indent=4, sort_keys=True, ensure_ascii=False))

if __name__ == "__main__":
    # func()
    #get_parents()
    check_export_path()