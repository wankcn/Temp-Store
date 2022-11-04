import os
import lupa
from lupa import LuaRuntime
import json

from data import toolData

class LuaSyntaxErrorExcetion(Exception):
    pass

def dump2dict(lua_tbl):
    if lupa.lua_type(lua_tbl) == 'table':
        return {k: dump2dict(v) for k, v in lua_tbl.items()}
    else:
        return lua_tbl

def syntax(src, dst):
    lua = LuaRuntime(unpack_returned_tuples=True)
    try:
        with open(src, encoding="utf8") as fd:
            code = fd.read()
            r = lua.compile(code)
            if toolData.get_value("export_json"):
                d = dump2dict(r())
                #with open(os.path.splitext(src)[0]+'.json', "w+") as fd2:
                with open(dst, "w+") as fd2:
                    fd2.write(json.dumps(d, indent=4))
    except Exception as e:
        print("【睁大眼睛看这里，别忽略】导出文件: ", src, "语法检查未通过，请去检查EXCEL：", e)
        raise LuaSyntaxErrorExcetion()

if __name__ == "__main__":
    syntax("H:\\Horcrux_Game\\L2_cn_trunk\\src\\data\\data\\activity_sign\\activitydata\\activity_sign_152_data.lua")