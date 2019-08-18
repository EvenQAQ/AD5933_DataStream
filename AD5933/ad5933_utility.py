import numpy as np
import csv
# from loess.loess_1d import loess_1d
from statsmodels.nonparametric.smoothers_lowess import lowess
from scipy import signal

class EvalData:
    # def __init__(self, filePath, num_of_steps):
    #     self.freqDict = {}
    #     self.filePath = filePath
    #     self.num = num_of_steps + 1
    #     self.impd = np.array([0.0]*self.num)
    #     self.phase = np.array([0.0]*self.num)
    #     self.real = np.array([0.0]*self.num)
    #     self.imag = np.array([0.0]*self.num)
    #     self.readFile()
    
    def __init__(self, full_list, num_of_steps):
        self.freqDict = {}
        self.num = num_of_steps + 1
        self.impd = np.array([0.0]*self.num)
        self.phase = np.array([0.0]*self.num)
        self.real = np.array([0.0]*self.num)
        self.imag = np.array([0.0]*self.num)
        self.read_stream(full_list)
    
    def read_stream(self, full_list):
        for data_list in full_list:
            freq = data_list[0]
            if freq not in self.freqDict:
                self.freqDict[freq] = {}
                self.freqDict[freq]['impd'] = np.array([float(data_list[1])])
                self.freqDict[freq]['phase'] = np.array([float(data_list[2])])
            else:
                self.freqDict[freq]['impd'] = np.append(
                    self.freqDict[freq]['impd'], float(data_list[1]))
                self.freqDict[freq]['phase'] = np.append(
                    self.freqDict[freq]['phase'], float(data_list[2]))
        i = 0
        for key in self.freqDict.keys():
            self.impd[i] = self.freqDict[key]['impd'].mean()
            # phase = self.freqDict[key]['phase'].mean()
            phase = self.freqDict[key]['phase'][0]
            # print(phase)
            self.phase[i] = phase*np.pi/180.
            i = i+1


    def update(self, freq, impd, phase):
        if freq not in self.freqDict:
            self.freqDict[freq] = {}
            self.freqDict[freq]['impd'] = np.array([float(impd)])
            self.freqDict[freq]['phase'] = np.array([float(phase)])
        else:
            self.freqDict[freq]['impd'] = np.append(self.freqDict[freq]['impd'], float(impd)) 
            self.freqDict[freq]['phase'] = np.append(self.freqDict[freq]['phase'], float(phase))
    
    def print_data(self):
        for key in self.freqDict.keys():
            print("Frequency: {} Hz".format(key))
            print("Impedance list: {}".format(self.freqDict[key]['impd']))
            print("Phase list: {}".format(self.freqDict[key]['phase']))

    def readFile(self):
        with open(self.filePath) as csvfile:
            reader = csv.DictReader(csvfile)
            for row in reader:
                if row['Frequency'] != 'Frequency':
                    self.update(int(row['Frequency']), row['Impedance'], row['Phase'])

        i = 0
        for key in self.freqDict.keys():
            self.impd[i] = self.freqDict[key]['impd'].mean()
            # phase = self.freqDict[key]['phase'].mean() 
            phase = self.freqDict[key]['phase'][0]
            # print(phase)
            self.phase[i] = phase*np.pi/180.
            i = i+1
    
    def impedance_calibration(self, cal_resistance, cal_c_hl):
        r = cal_resistance
        c = cal_c_hl     #3.5609e-13

        # xout, self.impd, weigts = loess_1d(np.arange(46), self.impd, rotate =True)
        b,  a = signal.butter(3, 0.6, btype='lowpass', analog=False) 
        # self.impd = lowess(self.impd, np.arange(46), frac=0.1)[:,1]
        # self.phase = lowess(self.phase, np.arange(46), frac=0.2)[:,1]

        for i in range(self.num):
            w = (i*2000 + 10000)*2*np.pi
            p1 = np.sqrt(1+r**2*c**2*w**2)
            z = self.impd[i]/p1
            # z = self.impd[i]
            # phase =  self.phase[i]
            phase = (self.phase[i] + np.arctan(-r*c*w))
            # if phase < 0:
            # print(phase)
                # dfsasdfasdf
            self.real[i] = z*np.cos(phase)
            self.imag[i] = z*np.sin(phase)

        # self.real = lowess(self.real, np.arange(46), frac=0.1)[:,1]
        # self.imag = lowess(self.imag, np.arange(46), frac=0.1)[:,1]
        # self.real = signal.filtfilt(b, a, self.real)
        # self.imag = signal.filtfilt(b, a, self.imag)
        # print(self.real)
        # xout, self.real, weigts = loess_1d(np.arange(46), self.real)
        # xout, self.imag, weigts = loess_1d(np.arange(46), self.imag)
    
def mirror_array(left, right):
    i = left.shape[0]
    target = np.array([0.0]*2*i)
    for j in range(i): 
        target[i+j] = right[j]
        target[i-j-1] = left[j]
    return target
