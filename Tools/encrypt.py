import re
import lupa
from lupa import LuaRuntime
import json
import ctypes

exclude_types = (126, )
key = 0xAF089F
mask = 0xFFFFFFFF

def dump_table(keys, lua_tbl):
    l = []
    for k in keys:
        v = lua_tbl[k]
        if isinstance(v, (int, float)):
            l.append("{} = {}".format(k, v))
        else:
            l.append('{} = "{}"'.format(k, v))
    return "{ %s }"%", ".join(l)

def encrypt(src, dst):
    lua = LuaRuntime(unpack_returned_tuples=True)
    outputs = []
    with open(src, encoding="utf8") as fd:
        code_list = []
        for line in fd.readlines():
            code_list.append(line)
        data_table = lua.execute("\n".join(code_list))
        outputs.append("local value_list = {} ")
        for i in range(len(code_list)):
            code_line = code_list[i]
            if code_line.startswith("value_list"):
                ret = re.match("value_list\[(\d+)\]", code_line)
                k = int(ret.groups()[0])
                v = data_table[k]
                if v.status_type not in exclude_types:
                    for i in range(1, 5):
                        attr = "value{}".format(i)
                        val = v[attr]
                        if val is not None:
                            if isinstance(val, (int, float)):
                                v[attr] = (ctypes.c_int32(int(val*10000)).value) ^ key
                keys = re.findall("(\w+)[\t ]*=.*?[, }]", code_line)
                outputs.append("value_list[{}] = {}".format(k, dump_table(keys, v)))
        outputs.append("")
        outputs.append("return value_list")
    if outputs:
        with open(dst, "w", encoding="utf8") as fd:
            fd.write("\n".join(outputs))

if __name__ == "__main__":
    encrypt("H:\\Horcrux_Game\\L2_cn_trunk\\src\\data2\\monsterskill\\status_speed_data.lua", '..\\src\\data\\monsterskill\\encrypt_speed_data.lua')
    # encrypt("..\\src\\data\\monsterskill\\status_speed_data.lua", '..\\src\\data\\monsterskill\\encrypt_speed_data.lua')