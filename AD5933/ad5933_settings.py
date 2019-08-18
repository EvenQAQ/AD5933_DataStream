import numpy as np
from lmfit import Model
from lmfit import Parameters
import csv
import matplotlib.pyplot as plt 
import matplotlib.ticker as ticker
from ad5933_utility import mirror_array
from scipy.optimize import fsolve,leastsq
from statsmodels.nonparametric.smoothers_lowess import lowess

fi_Rf = None
fi_Xf = None

class Settings:
    def __init__(self, total_r, num_of_steps, start_freq, freq_incr, r1 = None, r2 = None, mode = True): 
        ### frequency initialization
        self.mode = mode
        self.num = num_of_steps + 1
        self.start_freq = start_freq
        self.freq_incr = freq_incr
        self.freq_array = np.array([0.0]*self.num)

        for i in range(self.num):
            self.freq_array[i] = self.start_freq + self.freq_incr*i

        ### parameters
        self.total_r = total_r
        self.r1 = r1
        self.r2 = r2

        ### parasitics
        self.c_hl = 0.0
        self.c_ph = 0.0
        self.c_pl = 0.0
        self.c_g = 0.0
        self.c_x = 0.0

        ### for 2 point touch 

    def fit_parasitics(self, bg_real, bg_imag, color1, color2):
        i = self.num
        fit_impd = np.array([0.0]*2*i)
        fit_freq = np.array([0.0]*2*i)
        for j in range(i):
            fit_impd[i+j] = bg_real[j]
            fit_impd[i-j-1] = bg_imag[j]
            fit_freq[i+j] = 10000+j*2000
            fit_freq[i-j-1] = -10000 - j*2000
        
        ### fitting function
        ### offset resistor in series of DUT
        def fit_rz_p(C1, C2, R1, R2, Chl, Cg, w):
            step = np.heaviside(w,0.0)
            w= ((step-1)*w + step*w)
            I = 1j
            Z1 = 1/(C1*w*I + 1/R1)
            Z2 = 1/(C2*w*I + 1/R2)
            Zg = 1/(Cg*w*I)
            Zt = Z1 + Z2 + Z1*Z2/Zg
            Zm = Zt/(1+ Zt*Chl*w*I)
            
            real = Zm.real
            imag = Zm.imag
            return (1-step)*(imag) + step*(real)

        ### without offset resistor
        def fit_p(Cx, Rt, Chl, Cg, w, k):
            step = np.heaviside(w,0.0)
            w= ((step-1)*w + step*w)
            I = 1j
            Zx = 1/(Cx*w*I + 1/Rt)
            Zg = 1/(Cg*w*I)
            Zt = Zx + Zx*Zx*k*(1-k)/Zg

            Zm = Zt/(1+ Zt*Chl*w*I)
            
            real = Zm.real
            imag = Zm.imag
            return (1-step)*(imag) + step*(real)
    
        params = Parameters()

        ### offset resistor in series of DUT
        if self.mode is False:
            # params.add('C1', value = 8e-14, min = 1e-16, max=  1e-10)
            # params.add('C2', value = 8e-14, min = 1e-16, max=  1e-10)
            params.add('C1', value = 0, vary = False)
            params.add('C2', value = 0, vary = False)
            # params.add('R1', value = self.r1, min = self.r1-1e3, max = self.r1+1e3)
            # params.add('R2', value = self.r2, min = self.r2-1e3, max = self.r2+1e3)
            params.add('R1', value = self.r1, vary = False)
            params.add('R2', value = self.r2, vary = False)

        ### without offset resistor
        else:
            params.add('k', value = 0.21, min = 0.1, max = 0.4)
            # params.add('k', value = 0.21, vary = False)
            params.add('Cx', value = 1e-12, min = 1e-14, max=  1e-11, brute_step = 1e-13)
            params.add('Rt', value = self.total_r, min = self.total_r-1e4, max = self.total_r+1e4, brute_step = 100)
            # params.add('Rt', value = self.total_r, vary = False)

        # params.add('Cg', value = 2e-13, vary = False)
        params.add('Cg', value = 2e-13, min = 1e-13, max = 1e-11, brute_step = 1e-13)
        params.add('Chl', value = 2.5609e-13, min = 1e-15, max= 5e-13, brute_step = 1e-13)
        # params.add('Chl', value = 2.5e-13, vary = False) 

        if self.mode is True:
            model = Model(fit_p, independent_vars = ['w'])
            result = model.fit(fit_impd, params, method='linear_square', w = 2*np.pi*fit_freq)
            print(result.fit_report())
            ### update parasitics value
            self.c_hl = result.best_values['Chl']
            self.c_g = result.best_values['Cg']
            self.c_x = result.best_values['Cx']
        else:
            model = Model(fit_rz_p, independent_vars = ['w'])
            result = model.fit(fit_impd, params, method='linear_square', w = 2*np.pi*fit_freq)
            print(result.fit_report())
            ### update parasitics value
            self.c_hl = result.best_values['Chl']
            self.c_ph = result.best_values['C1']
            self.c_pl = result.best_values['C2']
            self.c_g = result.best_values['Cg']
        # plt.plot(fit_freq[46:], bg_real, color1+'o')
        # plt.plot(fit_freq[46:], result.best_fit[46:],color1)
        # plt.plot(fit_freq[46:], bg_imag, color2+'o')
        # plt.plot(fit_freq[46:], result.best_fit[:46][::-1],color2)

# for validation

        # plt.plot(fit_freq[46:], np.sqrt(bg_real**2 + bg_imag**2), color1)
        # plt.grid(True)
        # axes = plt.gca()
        # axes.set_xlim([fit_freq[46:].min(),fit_freq[46:].max()])
        # ax = plt.axes()
        # ax.xaxis.set_major_locator(ticker.MultipleLocator(20000))
###


        # ax.yaxis.set_major_locator(ticker.MultipleLocator(100000))
        # plt.plot(fit_freq[46:], np.sqrt(result.best_fit[46:]**2 + result.best_fit[:46][::-1]**2), '-')


    ### offset resistor in series of DUT
    def cal_rz_zf(self, a, cal_real, cal_imag):
        zf_real = np.array([0.0]*self.num)
        zf_imag = np.array([0.0]*self.num)
        R1 = self.r1
        R2 = self.r2
        C1 = self.c_ph
        C2 = self.c_pl
        Cg = self.c_g
        Chl = self.c_hl
        for i in range(self.num):
            w = (i*2000 + 10000)*2*np.pi
            I = 1j
            Zk = 1/(C1*w*I + 1/R1)
            Zt = 1/(C2*w*I + 1/R2)
            Za = Zt*a
            Zb = Zt*(1-a)
            Zg = 1/(Cg*w*I)
            Zm = cal_real[i] + cal_imag[i]*I
            Zc = Zm/(1-Zm*Chl*w*I)
            Zh = Zk+Za + Zk*Za/Zg
            Zp = Za + Zg + Zg*Za/Zk
            Zpf = Zh*Zb/(Zc-Zh-Zb)
            Zf = Zp*Zpf/(Zp-Zpf)
            zf_real[i] = Zf.real
            zf_imag[i] = Zf.imag
        return zf_real, zf_imag

    ### without offset resistor
    def cal_zf(self, a, cal_real, cal_imag):
        zf_real = np.array([0.0]*self.num)
        zf_imag = np.array([0.0]*self.num)

        ### try k value if needed
        k = 0.21
        Rt = self.total_r
        Cx = self.c_x
        Cg = self.c_g
        Chl = self.c_hl
        for i in range(self.num):
            w = (i*2000 + 10000)*2*np.pi
            I = 1j
            Zx = 1/(Cx*w*I + 1/Rt)
            Zk = Zx*k
            Za = Zx*(a-k)
            Zb = Zx*(1-a)
            Zg = 1/(Cg*w*I)
            Zm = cal_real[i] + cal_imag[i]*I
            Zc = Zm/(1-Zm*Chl*w*I)
            Zh = Zk+Za + Zk*Za/Zg
            Zp = Za + Zg + Zg*Za/Zk
            Zpf = Zh*Zb/(Zc-Zh-Zb)
            Zf = Zp*Zpf/(Zp-Zpf)
            zf_real[i] = Zf.real
            zf_imag[i] = Zf.imag
        return zf_real, zf_imag

    def plot_zf(self, zf_real, zf_imag):
        freq = mirror_array(-1*self.freq_array, self.freq_array)
        impd = mirror_array(zf_imag, zf_real)
        plt.plot(freq, impd)

    def plot_impd(self, zf_impd):
        freq = self.freq_array
        plt.plot(freq, zf_impd)

    ### fit Zf in order to get the position information
    def fit_pos(self, Rm, Xm, zf_real, zf_imag, name):
        Rm2 = mirror_array(Rm, Rm)
        Xm2 = mirror_array(Xm, Xm)
        zf_impd = mirror_array(zf_imag, zf_real)
        freq = mirror_array(-1*self.freq_array, self.freq_array)
        Cg = self.c_g
        Chl = self.c_hl
        C1 = self.c_ph
        C2 = self.c_pl
        Cx = self.c_x
        w = freq*2*np.pi
        step = np.heaviside(w,0.0)
        
        ### offset resistor in series of DUT
        def fit_rz(a, Cg, Chl, C1, C2, Rm, Xm, w, step):
            w= ((step-1)*w + step*w)
            I = 1j

            Zk = 1/(C1*w*I + 1/self.r1)
            Zt = 1/(C2*w*I + 1/self.r2)
            Za = Zt*a
            Zb = Zt*(1-a)
            Zg = 1/(Cg*w*I)
            Zm = Rm + Xm*I
            Zc = Zm/(1-Zm*Chl*w*I)
            Zh = Zk+Za + Zk*Za/Zg
            Zp = Za + Zg + Zg*Za/Zk
            Zpf = Zh*Zb/(Zc-Zh-Zb)
            Zf = Zp*Zpf/(Zp-Zpf)
            real = Zf.real
            imag = Zf.imag
        
            return (1-step)*imag + step*real     

        ### without offset resistor
        def fit(a, k, Cg, Chl, Cx, Rm, Xm, w, step):
            w= ((step-1)*w + step*w)
            I = 1j


            Zx = 1/(Cx*w*I + 1/self.total_r)
            Zk = Zx*k
            Za = Zx*(a-k)
            Zb = Zx*(1-a)
            Zg = 1/(Cg*w*I)
            Zm = Rm + Xm*I
            Zc = Zm/(1-Zm*Chl*w*I)
            Zh = Zk+Za + Zk*Za/Zg
            Zp = Za + Zg + Zg*Za/Zk
            Zpf = Zh*Zb/(Zc-Zh-Zb)
            Zf = Zp*Zpf/(Zp-Zpf)
            real = Zf.real
            imag = Zf.imag
        
            return (1-step)*imag + step*real  
        
        params = Parameters()
        if self.mode is False:
            # params.add('C1', value = C1, min = 0.5*C1, max = 10*C1, brute_step = 0.1*C1)
            # params.add('C2', value = C2, min = 0.5*C2, max = 10*C2, brute_step = 0.1*C2)
            params.add('C1', value = C1, vary = False)
            params.add('C2', value = C2, vary = False)
        else:
            # params.add('k', value = 0.21, vary = False)
            params.add('k', value = 0.21, min = 0.1 , max = 0.4, brute_step = 0.005)
            params.add('Cx', value = Cx, vary = False)
            # params.add('Cx', value = Cx, min = 0.1*Cx, max = 1000*Cx, brute_step = 0.1*Cx)
            

        params.add('Cg', value = Cg, min = 0.01*Cg, max = 100*Cg, brute_step = 0.1*Cg)
        # params.add('Chl', value = Chl, min = 0.1*Chl, max = 10*Chl, brute_step = 0.1*Chl)
        # params.add('Cg', value = Cg, vary = False)
        params.add('Chl', value = Chl, vary = False)
        params.add('a', value = 0.7, min = 0.5 , max = 1, brute_step = 0.005)
        # params.add('a', value = 0.71, vary = False)

        if self.mode is True:
            model = Model(fit, independent_vars = ['Rm', 'Xm', 'w', 'step'])
        else:
            model = Model(fit_rz, independent_vars = ['Rm', 'Xm', 'w', 'step'])
        result = model.fit(zf_impd, params, method = 'linear_square', Rm = Rm2 , Xm = Xm2 , w = w, step = step)
        # print("results from {}".format(name))
        print(result.best_values['a'])
        # print(result.fit_report())
        fit_result = lowess(result.best_fit, np.arange(92), frac=0.1)[:,1]
        # plt.plot(freq, fit_result)
        plt.plot(freq, result.best_fit)
        # return result.best_values['a']
    
    def try_plot(self, zf_real, zf_imag):
        ### try to draw the impedance from the calibrated Zf
        a_s = [0.5, 0.6, 0.7, 0.8, 0.9]
        I = 1j
        freq = self.freq_array
        w = freq*2*np.pi
        Chl = self.c_hl
        Cx = self.c_x
        Zf = zf_real + I*zf_imag
        Zx = 1/(Cx*w*I + 1/self.total_r)
        for a in a_s:
            Z1 = Zx*a
            Z2 = Zx*(1-a)
            Zm = Zx + Z1*Z2/Zf
            Zm = 1/(w*Chl*I + 1/Zm)
            self.plot_zf(Zm.real, Zm.imag)
    

class SettingsTwo:
    def __init__(self, total_r, num_of_steps, start_freq, freq_incr, r1 = None, r2 = None, mode = True): 
        ### frequency initialization
        self.mode = mode
        self.num = num_of_steps + 1
        self.start_freq = start_freq
        self.freq_incr = freq_incr
        self.freq_array = np.array([0.0]*self.num)

        for i in range(self.num):
            self.freq_array[i] = self.start_freq + self.freq_incr*i

        ### parameters
        self.total_r = total_r
        self.r1 = r1
        self.r2 = r2

        ### parasitics
        self.c_hl = 0.0
        self.c_ph = 0.0
        self.c_pl = 0.0
        self.c_x = 0.0


 
    def fit_parasitics(self, bg_real, bg_imag, color):
        i = self.num
        fit_impd = np.array([0.0]*2*i)
        fit_freq = np.array([0.0]*2*i)
        for j in range(i):
            fit_impd[i+j] = bg_real[j]
            fit_impd[i-j-1] = bg_imag[j]
            fit_freq[i+j] = 10000+j*2000
            fit_freq[i-j-1] = -10000 - j*2000
        
        ### fitting function
        ### offset resistor in series of DUT
        def fit_rz_p(C1, C2, R1, R2, Chl, Cg, w):
            step = np.heaviside(w,0.0)
            w= ((step-1)*w + step*w)
            I = 1j
            Z1 = 1/(C1*w*I + 1/R1)
            Z2 = 1/(C2*w*I + 1/R2)
            Zg = 1/(Cg*w*I)
            Zt = Z1 + Z2 + Z1*Z2/Zg
            Zm = Zt/(1+ Zt*Chl*w*I)
            
            real = Zm.real
            imag = Zm.imag
            return (1-step)*(imag) + step*(real)

        ### without offset resistor
        def fit_p(Cx, Rt, Chl, Cg, w, k):
            step = np.heaviside(w,0.0)
            w= ((step-1)*w + step*w)
            I = 1j
            Zx = 1/(Cx*w*I + 1/Rt)
            Zg = 1/(Cg*w*I)
            Zt = Zx + Zx*Zx*k*(1-k)/Zg

            Zm = Zt/(1+ Zt*Chl*w*I)
            
            real = Zm.real
            imag = Zm.imag
            return (1-step)*(imag) + step*(real)
    
        params = Parameters()

        ### offset resistor in series of DUT
        if self.mode is False:
            # params.add('C1', value = 8e-14, min = 1e-16, max=  1e-10)
            # params.add('C2', value = 8e-14, min = 1e-16, max=  1e-10)
            params.add('C1', value = 0, vary = False)
            params.add('C2', value = 0, vary = False)
            # params.add('R1', value = self.r1, min = self.r1-1e3, max = self.r1+1e3)
            # params.add('R2', value = self.r2, min = self.r2-1e3, max = self.r2+1e3)
            params.add('R1', value = self.r1, vary = False)
            params.add('R2', value = self.r2, vary = False)

        ### without offset resistor
        else:
            params.add('k', value = 0.21, min = 0.1, max = 0.4)
            # params.add('k', value = 0.21, vary = False)
            params.add('Cx', value = 1e-12, min = 1e-16, max=  1e-11, brute_step = 1e-13)
            params.add('Rt', value = self.total_r, min = self.total_r-1e4, max = self.total_r+1e4, brute_step = 100)
            # params.add('Rt', value = self.total_r, vary = False)

        # params.add('Cg', value = 2e-13, vary = False)
        params.add('Cg', value = 2e-13, min = 1e-13, max = 1e-11, brute_step = 1e-13)
        params.add('Chl', value = 2.5609e-13, min = 1e-14, max= 5e-13, brute_step = 1e-13)
        # params.add('Chl', value = 2.5e-13, vary = False) 

        if self.mode is True:
            model = Model(fit_p, independent_vars = ['w'])
            result = model.fit(fit_impd, params, method='linear_square', w = 2*np.pi*fit_freq)
            print(result.fit_report())
            ### update parasitics value
            self.c_hl = result.best_values['Chl']
            self.c_g = result.best_values['Cg']
            self.c_x = result.best_values['Cx']
        else:
            model = Model(fit_rz_p, independent_vars = ['w'])
            result = model.fit(fit_impd, params, method='linear_square', w = 2*np.pi*fit_freq)
            print(result.fit_report())
            ### update parasitics value
            self.c_hl = result.best_values['Chl']
            self.c_ph = result.best_values['C1']
            self.c_pl = result.best_values['C2']
            self.c_g = result.best_values['Cg']
        # plt.plot(fit_freq[46:], bg_real, color+'.')
        # plt.plot(fit_freq[46:], bg_imag, color+'.')
        # plt.plot(fit_freq[46:], result.best_fit[46:],color)
        # plt.plot(fit_freq[46:], result.best_fit[:46][::-1], color)
        # plt.plot(fit_freq[46:], np.sqrt(bg_real**2 + bg_imag**2), color+'o')
        # plt.plot(fit_freq[46:], np.sqrt(result.best_fit[46:]**2 + result.best_fit[:46][::-1]**2), color+ '-')


    def cal_zf(self, a, b, cal_real, cal_imag):
        zf_real = np.array([0.0]*self.num)
        zf_imag = np.array([0.0]*self.num)
        # self.plot_zf(cal_real,cal_imag)
        ### try k value if needed
        b = a + b
        Rt = self.total_r
        Cx = self.c_x
        Chl = self.c_hl
        Cg = self.c_g
        ##########
        Rm2 = mirror_array(cal_real, cal_real)
        Xm2 = mirror_array(cal_imag, cal_imag)
        freq = mirror_array(1*self.freq_array, self.freq_array)
        w = freq*2*np.pi
        I = 1j
        Zm = Rm2 + I*Xm2
        Zx = self.total_r/(1+self.total_r*Cx*w*I)
        ### considering calibration error
        Zm = Zm/(1-Zm*Chl*w*I)
        Zf = 0.25*self.total_r**2/(Zm - self.total_r)
        # Zf = Zm
        # ## use rf xf as initial value to find the root of nonlineal systems of equations
        Z1 = Zx*a
        Z2 = Zx*(b-a)
        Z3 = Zx*(1-b)
        Zf = (Zf.real, Zf.imag)
        Zm = (Zm.real, Zm.imag)
        Zg = 1/(Cg*w*I)
        
        def func(Z_f, Zm, Z1, Z2, Z3, Zg):
            Rf, Xf = Z_f[:92], Z_f[92:]
            Rm, Xm = Zm[0], Zm[1]
            I = 1j
            Zf = Rf + Xf*I
            Zf2 = Zf*Z2/(Z2+2*Zf) 
            Zff = Zf*Zf/(Z2+2*Zf)
            Zh = Z1+Zf2
            Zl = Z3+Zf2
            Zs = Zff*Zg/(Zff+Zg)
            Z_m = Zh + Zl + Zh*Zl/Zs
            return np.array([Z_m.real -Rm, Z_m.imag -Xm]).ravel()
           
        Z_f,flag = leastsq(func, Zf, args = (Zm, Z1, Z2, Z3, Zg), xtol = 0.001,ftol = 0.001)
        # Z_f = fsolve(func, Zf, args = (Zm, Z1, Z2, Z3, Zg), xtol = 0.1)
        # print(Z_f)
        zf_real = Z_f[:46][::-1]
        zf_imag = Z_f[138:]
        
        ### test plot
        I = 1j
        Zx = self.total_r/(Cx*w*I*self.total_r + 1)
        ### considering calibration error
        freq = mirror_array(-1*self.freq_array,self.freq_array)
        step = np.heaviside(w,0.0)  
        zf_r = mirror_array(zf_real,zf_real)
        zf_x = mirror_array(zf_imag,zf_imag)
        Z1 = Zx*a
        Z2 = Zx*(b-a)
        Z3 = Zx*(1-b)
        Zg = 1/(Cg*w*I)
        Zf = zf_r + I*zf_x
        Zf2 = Zf*Z2/(Z2+2*Zf) 
        Zff = Zf*Zf/(Z2+2*Zf)
        Zh = Z1+Zf2
        Zl = Z3+Zf2
        Zs = Zff*Zg/(Zff+Zg)
        Zm = Zh + Zl + Zh*Zl/Zs
        Zm = Zm/(Chl*w*I*Zm + 1)
        
        # self.plot_zf((Zm.real[46:]),(Zm.imag[46:]))

        ###########
        # for i in range(self.num):
        #     w = (i*2000 + 10000)*2*np.pi
        #     I = 1j
        #     Zx = 1/(Cx*w*I + 1/Rt)
        #     Zm = cal_real[i] + I*cal_imag[i]
        #     ### considering calibration error
        #     Zm = Zm/(1-Zm*Chl*w*I)
        #     Zf = 0.25*self.total_r**2/(Zm - self.total_r)
        #     Zm = (Zm.real, Zm.imag)
            
        #     ### formal approach
        #     Z1 = Zx*(a)
        #     Z2 = Zx*(b-a)
        #     Z3 = Zx*(1-b)
        #     Zg = 1/(Cg*w*I)
        #     Zf = (Zf.real, Zf.imag)
        #     # Zf = (-300000, -300000)
            
        #     def func(Zf, Zm, Z1, Z2, Z3, Zg):
        #         Rf, Xf = Zf
        #         Rm, Xm = Zm
        #         I = 1j
        #         Zf = Rf + Xf*I
        #         Zf2 = Zf*Z2/(Z2+2*Zf) 
        #         Zff = Zf*Zf/(Z2+2*Zf)
        #         Zh = Z1+Zf2
        #         Zl = Z3+Zf2
        #         Zs = Zg*Zff/(Zg+Zf)

        #         Zm = Zh + Zl + Zh*Zl/Zs
        #         return Rm - Zm.real, Xm - Zm.imag
                
        #     Zf = fsolve(func, Zf,(Zm, Z1, Z2, Z3, Zg), xtol = 100)
        #     zf_real[i] = Zf[0]
        #     zf_imag[i] = Zf[1]
        return zf_real, zf_imag


    def plot_zf(self, zf_real, zf_imag):
        freq = mirror_array(-1*self.freq_array, self.freq_array)
        impd = mirror_array(zf_imag, zf_real)
        plt.plot(freq, impd)


    ### fit Zf in order to get two positions information
    def fit_pos(self, Rm, Xm, zf_real, zf_imag, name):
        # global fi_Rf, fi_Xf
        # Zmes = mirror_array(Xm, Rm)
        # Rm2 = mirror_array(Rm, Rm)
        # Xm2 = mirror_array(Xm, Xm)
        # zf_impd = mirror_array(zf_imag, zf_real)
        # freq = mirror_array(-1*self.freq_array, self.freq_array)
        # Chl = self.c_hl
        # C1 = self.c_ph
        # C2 = self.c_pl
        # Cx = self.c_x
        # Cg = self.c_g
        # w = freq*2*np.pi
        # step = np.heaviside(w,0.0)    
      
        # def fit(a, b, Chl, Cg, Rm, Xm, w, step, fi_Rf, fi_Xf):
        #     w= ((step-1)*w + step*w)
        #     I = 1j
        #     a = b*a
        #     if a < 0.5:
        #         a = 0.5
        #     Zm = Rm + I*Xm
        #     Zx = 1/(Cx*w*I + 1/self.total_r)
        #     ### considering calibration error
        #     Zm = Zm/(1-Zm*Chl*w*I)

        #     if fi_Rf == None and fi_Xf == None:
        #         Zf = 0.25*self.total_r**2/(Zm - self.total_r)
        #         fi_Rf = Zf.real
        #         fi_Xf = Zf.imag
        #     # ## use rf xf as initial value to find the root of nonlineal systems of equations

        #     Z1 = Zx*a
        #     Z2 = Zx*(b-a)
        #     Z3 = Zx*(1-b)
        #     Zf = (fi_Rf, fi_Xf)
        #     Zm = (Zm.real, Zm.imag)
        #     Zg = 1/(Cg*w*I)
        #     def func(Z_f, Zm, Z1, Z2, Z3, Zg):
        #         Rf, Xf = Z_f[:92], Z_f[92:]
        #         Rm, Xm = Zm[0], Zm[1]
        #         I = 1j
        #         Zf = Rf + Xf*I
        #         Zf2 = Zf*Z2/(Z2+2*Zf) 
        #         Zff = Zf*Zf/(Z2+2*Zf)
        #         Zh = Z1+Zf2
        #         Zl = Z3+Zf2
        #         Zs = Zff*Zg/(Zff+Zg)
        #         Zm = Zh + Zl + Zh*Zl/Zs
        #         return np.array([Zm.real -Rm, Zm.imag -Xm]).ravel()
                
        #     Zf, flag = leastsq(func, Zf, args = (Zm, Z1, Z2, Z3, Zg), xtol = 0.1)
        #     fi_Rf = Zf[:92]
        #     fi_Xf = Zf[92:]

        #     return (1-step)*fi_Xf + step*fi_Rf

        ########## New try
        I = 1j
        Z_fit = mirror_array(np.sqrt(Xm**2 + Rm**2),np.sqrt(Xm**2 + Rm**2))
        # Z_fit = mirror_array(Xm,Rm)
        zf_imag = mirror_array(zf_imag, zf_imag)
        zf_real = mirror_array(zf_real, zf_real)
        # zf_impd = zf_real + I*zf_imag 
        freq = mirror_array(-1*self.freq_array,self.freq_array)
        Chl = self.c_hl
        C1 = self.c_ph
        C2 = self.c_pl
        Cx = self.c_x
        Cg = self.c_g
        w = freq*2*np.pi 
        step = np.heaviside(w,0.0)  

        def fit(a, b, Chl, Cg, Cx, w, step, zf_imag, zf_real,Zf):
            w= ((step-1)*w + step*w)
            I = 1j
            a = a*b
            if a < 0.5:
                a = 0.5
            Zx = self.total_r/(1+self.total_r*Cx*w*I)
            ### considering calibration error
            
            Z1 = Zx*a
            Z2 = Zx*(b-a)
            Z3 = Zx*(1-b)
            Zg = 1/(Cg*w*I)
            Zf = zf_real + I*zf_imag
            Zf2 = Zf*Z2/(Z2+2*Zf) 
            Zff = Zf*Zf/(Z2+2*Zf)
            Zh = Z1+Zf2
            Zl = Z3+Zf2
            Zs = Zff*Zg/(Zff+Zg)
            Zm = Zh + Zl + Zh*Zl/Zs
            Zm = Zm/(Chl*w*I*Zm + 1)
            z = np.sqrt(Zm.imag**2 + Zm.real**2)
            # print((1-step)*Zm.imag + step*Zm.real - Zf)
            # return (1-step)*Zm.imag + step*Zm.real
            return z

        params = Parameters()
        params.add('Cx', value = Cx, vary = False)
        # params.add('Cx', value = Cx, min = 0.1*Cx, max = 10*Cx, brute_step = 0.5*Cx)
        params.add('Chl', value = Chl, vary = False)
        # params.add('Cg', value = Cg, vary = False)
        params.add('Cg', value = Cg, min = 0.5*Cg, max = 2*Cg, brute_step = 0.5*Cg)
        # params.add('Chl', value = Chl, min = 0.5*Chl, max = 2*Chl, brute_step = 0.5*Chl)
        params.add('a', value = 0.6, min = 0.6 , max = 0.85, brute_step = 0.01)
        # params.add('a', value = 0.7, vary = False)
        # params.add('b', value = 0.7, vary = False)
        params.add('b', value = 0.6, min = 0.6, max = 0.90, brute_step = 0.01)
        model = Model(fit, independent_vars = ['w', 'step', 'zf_imag', 'zf_real', 'Zf'])
        result = model.fit(Z_fit, params, method = 'differential_evolution',  w = w, step = step, zf_imag = zf_imag, zf_real = zf_real, Zf =Z_fit )
        # print("results from {}".format(name))
        if result.best_values['b']*result.best_values['a'] < 0.5:
            print("0.5")
        else:
            print(result.best_values['b']*result.best_values['a']) 
        print(result.best_values['b'])
        # print(result.fit_report())
        plt.plot(freq, Z_fit,'-')
        plt.plot(freq, result.best_fit,'-')

        ####

        ########## New try


        
        # params = Parameters()
        # # params.add('k', value = 0.21, vary = False)
        # params.add('Cx', value = Cx, vary = False)
        # # params.add('Cx', value = Cx, min = 0.1*Cx, max = 10*Cx, brute_step = 0.5*Cx)
        # params.add('Chl', value = Chl, vary = False)
        # # params.add('Cg', value = Cg, vary = False)
        # params.add('Cg', value = Cg, min = 0.5*Cg, max = 3*Cg, brute_step = 0.5*Cg)
        # # params.add('Chl', value = Chl, min = 0.5*Chl, max = 100*Chl, brute_step = 0.5*Chl)
        # params.add('a', value = 0.5, min = 0.5 , max = 0.9, brute_step = 0.02)
        # # params.add('a', value = 0.7, vary = False)
        # # params.add('b', value = 0.7, vary = False)
        # params.add('b', value = 0.6, min = 0.6, max = 1.0, brute_step = 0.02)
        # model = Model(fit, independent_vars = ['Rm', 'Xm', 'w', 'step', 'fi_Rf', 'fi_Xf'])
        # result = model.fit(zf_impd, params, method = 'brute', Rm = Rm2 , Xm = Xm2 , w = w, step = step, fi_Rf = fi_Rf, fi_Xf = fi_Xf)
        # # print("results from {}".format(name))
        # if result.best_values['b']*result.best_values['a'] < 0.5:
        #     print(0.5)
        # else:
        #     print(result.best_values['b']*result.best_values['a']) 
        # print(result.best_values['b'])
        # # print(result.fit_report())
        # plt.plot(freq, result.best_fit)

        # fi_Rf = None
        # fi_Xf = None

    def try_plot(self, zf_real, zf_imag):
        ### try to draw the impedance from the calibrated Zf
        a_s = [0.5, 0.5, 0.5]
        b_s = [0.6, 0.75, 0.9]
        I = 1j
        freq = self.freq_array
        w = freq*2*np.pi
        Chl = self.c_hl
        Cx = self.c_x
        Zf = zf_real + I*zf_imag
        Zx = 1/(Cx*w*I + 1/self.total_r)
        for a, b in zip(a_s, b_s):
            Z1 = Zx*a
            Z2 = Zx*(b-a)
            Z3 = Zx*(1-b)
            Zf2 = Z2*Zf/(Z2 + 2*Zf)
            Zff = Zf*Zf/(Z2 + 2*Zf)
            Zh = Z1 + Zf2
            Zl = Z3 + Zf2
            Zm = Zh + Zl + Zh*Zl/Zff
            ### consider Chl
            Zm = 1/(w*Chl*I + 1/Zm)
            self.plot_zf(Zm.real, Zm.imag)

