import sys, serial, argparse, os
import numpy as np
import time
import threading
import csv
from lmfit import Model
from lmfit import Parameters
import matplotlib.pyplot as plt 
import os
from ad5933_settings import Settings
from ad5933_utility import EvalData
import socket


cal_resistance = 677000
# cal_c_hl = 7.3e-13 #### 25cm
cal_c_hl =    7.3e-13 #### 25cm
# cal_c_hl =    8e-13 #### 25cm
CONF_MES = bytes("confirmed...#", encoding="utf-8")

STEPSIZE = 46
BUFFERSIZE = 3 * STEPSIZE
GROUNDSIZE = 5 * STEPSIZE


def line2list(mes):
    data_list = []
    strings = mes.split("#")
    for line in strings:
        if line == '':
            continue
        string = line.split(",")
        data_list.append([int(string[0]), float(string[1]), float(string[2])])
    # print(data_list)
    return data_list

# main() function
def main():
    

    user_name = "zhao"
    form= "stripe"      

    ### read background profile, still need to calibrate for real and imag data
    # background = EvalData("../study/evaluation_accuracy/" + user_name + "/" + form + "/single/background.csv", 45)
    # background = EvalData("../study/finger_condition/cord/wet/background.csv", 45)
    
    back_data = ""
    back_list = []
    cali_data = ""
    cali_list = []
    buffer_data = ""
    buffer_list = []

    server = socket.socket()
    host = "10.221.55.7"
    port = 1213
    server.setsockopt(socket.SOL_SOCKET, socket.SO_REUSEADDR, 1)
    server.bind(("localhost", port))
    server.listen(1)
    print("server init finished...")
    back_count = 0
    cali_count = 0
    buffer_count = 0
    while 1:
        conn, addr = server.accept()
        print(conn, addr)
        conn.send(bytes("server ready...", encoding="utf-8"))

        # get background 
        while 1:
            if conn.recv(1024) != CONF_MES:
                continue
            else:
                break
        while back_count < GROUNDSIZE:
            try:
                back_data += str(conn.recv(1024), encoding = "utf-8")
                back_count += 1
            except:
                print("没收到，有点凉")
                break

        print("got background, processing...")
        back_list = line2list(back_data)
        for i in back_list:
            print(i)
        background = EvalData(back_list, 45)
        background.impedance_calibration(cal_resistance, cal_c_hl)
        ### determine parastic components value of corresponding settings
        if form == "cord":
            settings = Settings(660000, 45, 10000, 2000)
        else:
            settings = Settings(640000, 45, 10000, 2000)

        settings.fit_parasitics(background.real, background.imag, 'b', 'g')
        optNameList = []
        print("finished getting background...")
        back_data = ""
        back_list = []

        # calibration
        while 1:
            if conn.recv(1024) != CONF_MES:
                continue
            else:
                break

        while cali_count < BUFFERSIZE:
            try:
                cali_data += str(conn.recv(1024), encoding = "utf-8")
                cali_count += 1
            except:
                print("没收到，有点凉")
                break

        cali_list = line2list(cali_data)
        cal_touch = EvalData(cali_list, 45)
        cal_touch.impedance_calibration(cal_resistance, cal_c_hl)
        zf_real, zf_imag = settings.cal_zf(
            0.5, cal_touch.real, cal_touch.imag)
        settings.plot_zf(zf_real, zf_imag)
        optNameList.append("cal")
        # plt.legend(optNameList, loc='lower right')
        # plt.show()
        for i in cali_list:
            print(i)
        print("finished calibration")
        cali_data = ""
        cali_list = []

        # one test
        number = 0
        while 1:
            try:
                while 1:
                    if conn.recv(1024) != CONF_MES:
                        continue
                    else:
                        break
                co = 0
                while co < BUFFERSIZE:
                    try:
                        buffer_data += str(conn.recv(1024), encoding = "utf-8"
                        )
                        co += 1
                    except:
                        print("没收到，有点凉") 
                # use stream data
                buffer_list = line2list(buffer_data)
                touch_tmp = EvalData(buffer_list, 45)
                touch_tmp.impedance_calibration(cal_resistance, cal_c_hl)
                # settings.plot_zf(touch_tmp.real, touch_tmp.imag)
                # settings.plot_zf(touch_tmp.impd, touch_tmp.impd)
                settings.fit_pos(touch_tmp.real, touch_tmp.imag,
                            zf_real, zf_imag, str(number))
                optNameList.append(str(number))
                # for i in buffer_list:
                #     print(i)
                number += 1
                print("the " + number + " time touch")
                buffer_data = ""
                buffer_list.clear()

                plt.legend(optNameList, loc='lower right')
                plt.show()


                conn.send(CONF_MES) 
            except:
                print("loop client closed 没凉...")
                
        conn.close()
    server.close()
    print("done")


   



# call main
if __name__ == "__main__":
    main()
