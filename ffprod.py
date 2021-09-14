# -*- coding: utf-8 -*-
"""
Created on Mon Sep 13 01:53:48 2021

@author: billstark001
"""

import numpy as np
import pandas as pd

from collections import defaultdict

'''
state:
    (N, P, Q, E, Z, I, H, L)
    N: work time
    P: process rate
    Q: quality
    E: endurance
    Z: production force
    I: inner static
    H: current quality 0=low, 1=normal, 2=high, 3=highest
    L: dict of additive effects
'''

def read_data(path):
    c1 = pd.read_csv(open(path, 'r', encoding="gbk"))
    c2 = []
    for i in range(len(c1)):
        c = c1.loc[i]
        d = {}
        t = {}
        for k in c1.columns:
            if k in key_whitelist:
                d[k] = c[k]
            else:
                t[k] = int(c[k]) if not np.isnan(c[k]) else 0
        if isinstance(d['sp_eff'], float) and np.isnan(d['sp_eff']):
            d['sp_eff'] = []
        else:
            d['sp_eff'] = d['sp_eff'].split(';')
        d['timed_eff'] = t
        c2.append(d)
    return c2

def get_opr(name, oprs, level=80):
    ans = None
    for o in oprs:
        if o['least_level'] > level:
            continue
        elif o['name'] == name:
            if ans == None or ans['least_level'] < level:
                ans = o
    return ans

inner_static_rate = [
 1.0, 
 1.0, 
 1.2388804146556174,
 1.4932383207358164,
 1.7631515041131651,
 2.0476428983395563,
 2.3481392101508662,
 2.6622366663631123,
 2.9923390401602767,
 3.3379966912545913,
 3.698682193485718,
 4.074395546853657]

key_whitelist = [
    'name', 
    'least_level', 
    'd_process', 
    'd_quality', 
    'success_rate', 
    'd_force', 
    'd_endurance', 
    'd_inner_static', 
    'sp_eff']

oprs = read_data('data.csv')

init_state = lambda: (0, 0, 0, 0, 0, 0, 1, {})

hq_dict = [0.5, 1, 1.5, 2.5]

def simulate_seq(opr, seq, goal, max_count=1000, hq_rate_dict = [0, 0.65, 0.25, 0.1]):
    ans = []
    failed_opr = 0
    for i, (r, state) in enumerate(seq):
        sim_succ, sim_res = simulate_state(opr, state, goal, r, hq_rate_dict)
        ans += sim_res
        if not sim_succ:
            failed_opr += 1
        if len(ans) > max_count:
            p = np.array([x[0] for x in ans])
            p /= np.sum(p)
            #ans_ = np.random.choice(len(ans), size=max_count, p=p)
            ans__ = defaultdict(int)
            selected_count = 0
            while len(ans__) < max_count and selected_count < 4:
                aid = np.random.choice(len(ans), size=max_count * 2, p=p)
                for a in aid: ans__[a] += 1
                selected_count += 1
            ans = [[ans__[x] / (selected_count * max_count * 2), ans[x][1]] for x in ans__]
            #ans = sorted(ans, key=lambda x: x[0], reverse=True) # by probability
            #ans = ans[:max_count]
    failed_rate = failed_opr / len(seq)
    return ans, failed_rate

def eval_seq(sim):
    ans1 = 0
    ans2 = np.zeros((4, ), dtype=float)
    for res in sim:
        ans1 += res[0]
        ans2 += res[0] * np.array(res[1][1: 5], dtype=float)
    ans2 /= ans1
    return ans1, ans2

def simulate_state(opr, state, goal, total_rate=1, hq_rate_dict = [0, 0.65, 0.25, 0.1]):
    
    if opr == None:
        return False, [[total_rate, state]]
    
    N, P, Q, E, Z, I, H, L = state
    GP, GQ, GE, GZ = goal
    L = dict(L)
    
    # judge if operation is legal
    
    flag = True
    if 'only_first' in opr['sp_eff'] and N > 0: flag = False
    if 'hq_only' in opr['sp_eff'] and H < 2: flag = False
    if 'element_mark_appended' in opr['sp_eff'] and 'element_mark_appended' in L: flag = False
    if 'not_in_frugal' in opr['sp_eff'] and 'endurance_50p' in L: flag = False
    if 'only_when_inner_static' in opr['sp_eff'] and I == 0: flag = False
    if 'add_inner_static' in opr['sp_eff'] and I > 0: flag = False
    
    # judge if the state is halting
    flag2 = True
    if E >= GE: flag2 = False
    if P >= GP: flag2 = False
    if Z + opr['d_force'] > GZ: flag2 = False
    
    flag = flag and flag2
    
    if not flag:
        return flag, [[total_rate, state]]
    
    # calculate p q e s
    p_rate = 1
    q_rate = 1
    e_rate = 1
    
    if 'd_quality_add_20_by_inner_static' in opr['sp_eff'] and I > 0:
        q_rate += 0.2 * (I - 1)
    
    if 'process_200p_su' in L: p_rate += 1
    if 'quality_200p_su' in L: q_rate += 1
    if 'process_150p' in L: p_rate += 0.5
    if 'quality_150p' in L: q_rate += 0.5
    if 'endurance_50p' in L: e_rate -= 0.5
    
    q_rate *= inner_static_rate[I]
    q_rate *= hq_dict[H]
    
    if 'element_mark' in L: 
        if 'element_mark_applied' in opr['sp_eff']: p_rate += 2 * max(1 - P / GP, 0)
    
    if 'eff_50p_if_not_enough_endurance' in opr['sp_eff']:
        if opr['d_endurance'] * e_rate + E > GE: p_rate *= 0.5
        
    s_rate = opr['success_rate']
    if 'succ_to_100_if_after_observation' in opr['sp_eff']:
        if 'after_observation' in L: s_rate = 1
    
    df = opr['d_force']
    if 'force_to_18_after_processing' in opr['sp_eff']:
        if 'after_processing' in L: df = 18
    
    # execute current operation
    
    P_ = P + opr['d_process'] * p_rate
    Q_ = Q + opr['d_quality'] * q_rate
    E = E + opr['d_endurance'] * e_rate
    Z = Z + df
    I_ = I + opr['d_inner_static'] if I > 0 or 'add_inner_static' in opr['sp_eff'] else 0
    
    if 'endurance_add5' in L and not ('no_work_time' in opr['sp_eff']):
        if E < GE: E -= 5
    
    if 'reset_inner_static' in opr['sp_eff']:
        I_ = 0
    if 'div_inner_static' in opr['sp_eff']:
        I_ = I * 2
        I = int(I / 2) + 1
        
    
        
    # if I > 11: I = 11
    if I_ > 11: I_ = 11
    if Z < 0: Z = 0
    
    if not ('no_work_time' in opr['sp_eff']):  
        N = N + 1
        for l in list(L.keys()): 
            L[l] -= 1
            if L[l] == 0:
                del L[l]
            elif L[l] < 0:
                raise Exception("damn")
            
    L_ = dict(L)

    if 'do_not_complete' in L_ and P_ > GP: 
        P_ = GP - 1
        del L_['do_not_complete']

    if 'process_200p_su' in L_ and opr['d_process'] > 0:
        del L_['process_200p_su']
        
    if 'quality_200p_su' in L_ and opr['d_quality'] > 0:
        del L_['quality_200p_su']
        
    # add effects
    
    for eff in opr['timed_eff']:
        oe = opr['timed_eff'][eff]
        if oe > 0:
            L_[eff] = oe
        
    anss = [[(1 - s_rate) * total_rate, (N, P, Q, E, Z, I, H, L)], [s_rate * total_rate, (N, P_, Q_, E, Z, I_, H, L_)]]
    
    # divide H
    
    ansss = []
    for ans in anss:
        a, b = ans
        if a <= 0: continue
        n, p, q, e, z, i, h, l = b
            
        if h == 3: 
            ansss.append([a, (n, p, q, e, z, i, 0, l)])
        # doubtful
        elif h != 1: 
            ansss.append([a, (n, p, q, e, z, i, 1, l)])
        else: 
            for h_, hr in zip([0, 1, 2, 3], hq_rate_dict):
                if hr <= 0: continue
                ansss.append([a * hr, (n, p, q, e, z, i, h_, l)])
    
    # update L by operation
    return flag, ansss

# condition related

# craftsmanship, control
def calculate_base_effect(d_level, cm, ct, suggested_cm, suggested_ct) :
    # level difference factor
    d_level = min(max(-30, d_level), 20)
    if d_level < -20:
        ldf_cm = 0.8 + 0.02 * (d_level + 30)
        ldf_ct = 0.6 + 0.04 * (d_level + 30)
    elif d_level > 0:
        ldf_cm = [1, 1.05, 1.1, 1.15, 1.2, 1.25, 1.27, 1.29, 1.31, 1.33, 
                  1.35, 1.37, 1.39, 1.41, 1.43, 1.45, 1.46, 1.47, 1.48, 1.49, 1.5][d_level]
        ldf_ct = 1
    else:
        ldf_cm = ldf_ct = 1    
    
    ans_cm = int((ldf_cm * (0.21 * cm + 2) * (10000 + cm)) / (10000 + suggested_cm))
    ans_ct = int((ldf_ct * (0.35 * ct + 35) * (10000 + ct)) / (10000 + suggested_ct))
    
    return ans_cm, ans_ct


def get_goal_by_data(ep, eq, pp, qq, e, z):
    p = 100 * pp / ep
    q = 100 * qq / eq
    return (p, q, e, z)

def eval_on_average(m1, goal, hq_rate_dict=hq_dict):
    sss = [(1, init_state())]
    print ('# 技能 [制作进度 加工进度 耐久 制作力] 失败率 样本数')
    mult = np.array([1, 1, -1, -1])
    diff = np.array([0, 0, goal[2], goal[3]])
    m2 = [get_opr(x, oprs) for x in m1]
    for i in range(len(m2)):
        m = m2[i]
        sss, fr = simulate_seq(m, sss, goal, max_count=1000, hq_rate_dict=hq_rate_dict)
        print(i + 1, m['name'], (np.around(eval_seq(sss)[1], decimals=3)*mult+diff), fr, len(sss))
    print (f'条件 {np.array(goal)}')

def output(m):
    if len(m) > 15:
        ans = []
        anss = ''
        for i in range(len(m)):
            if i % 14 == 0 and i > 0:
                anss += f'/e "Macro {int(i / 14)} Completed."'
                ans.append(anss)
                anss = ''
            anss += f'/ac "{m[i]}" <wait.3>\n'
        anss += f'/e "Macro {len(ans) + 1} Completed."'
        ans.append(anss)
        return ans
    ans = ''
    for s in m:
        ans += f'/ac "{s}" <wait.3>\n'
    return [ans[:-1]]

if __name__ == "__main__":
    
    #goal = (1265.5251141552512, 5325.375939849624, 70, 507)
    goal = get_goal_by_data(447, 552, 7414, 46553, 70, 522) # 唯美装备
    mk = get_opr('制作', oprs)
    hrd = [0, 1, 0, 0]
        
    m1 = ['坚信', '内静', '改革', '加工', '加工', '加工', '加工', '阔步', '精修', '改革', '中级加工', '中级加工', '阔步', '比尔格的祝福', '制作']
    m1 = ['闲静',
 '加工',
 '俭约加工',
 '加工',
 '掌握',
 '改革',
 '长期俭约',
 '坯料加工',
 '坯料加工',
 '比尔格的祝福',
 '坯料制作',
 '崇敬',
 '坯料制作',
 '坯料制作',
 '坯料制作',
 '制作']
    
    for m in output(m1): print(m)
    eval_on_average(m1, goal)
    eval_on_average(m1, goal, hrd)

    
                
    