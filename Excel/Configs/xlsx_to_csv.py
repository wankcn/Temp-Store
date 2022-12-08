#!/usr/bin/env python

# 需要安装Python3
# cmd 执行  pip install xlrd2 

import xlrd2
import csv
import codecs
import os

DATA_PATH = os.getcwd() + '/Datas/'


def xlsx_to_csv(name):
    src = DATA_PATH + name + '.xlsx'
    res = DATA_PATH + name + '.csv'

    workbook = xlrd2.open_workbook(src)
    table = workbook.sheet_by_index(0)
    with codecs.open(res, 'w', encoding='ansi') as f:
        write = csv.writer(f)
        for row_num in range(table.nrows):
            row_value = table.row_values(row_num)
            if row_value[5] == 1:
                row_value[5] = 'TRUE'
            elif row_value[5] == 0:
                row_value[5] = 'FALSE'
            write.writerow(row_value)


if __name__ == '__main__':
    xlsx_to_csv("__tables__")
