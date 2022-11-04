from PyQt5 import QtCore, QtGui, QtWidgets

class TableHeader(QtWidgets.QHeaderView):
    
    checkBoxChanged = QtCore.pyqtSignal(bool)

    def __init__(self, orientation, parent=None):
        # self.isOn = False
        # self.option = QtWidgets.QStyleOptionButton()
        super().__init__(orientation, parent)

    # def paintSection(self, painter, rect, logicalIndex):
    #     painter.save()
    #     super().paintSection(painter, rect, logicalIndex)
    #     painter.restore()
    #     if logicalIndex == 0:
    #         self.option.rect = QtCore.QRect(0, 0, 20, 20)
    #         if self.isOn:
    #             self.option.state = QtWidgets.QStyle.State_On
    #         else:
    #             self.option.state = QtWidgets.QStyle.State_Off
    #         self.style().drawPrimitive(QtWidgets.QStyle.PE_IndicatorCheckBox, self.option, painter)
    
    # def mousePressEvent(self, event):
    #     # import time
    #     # print(time.time())
    #     # print(event.pos(), self.option.rect)
    #     if self.logicalIndexAt(event.pos()) == 0 and self.option.rect.contains(event.pos()):
    #         self.isOn = not self.isOn
    #         self.updateSection(0)
    #         self.checkBoxChanged.emit(self.isOn)
    #     super().mousePressEvent(event)