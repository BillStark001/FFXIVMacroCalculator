# -*- coding: utf-8 -*-
"""
Created on Mon Sep 13 01:53:48 2021

@author: zhaoj
"""

#import tensorflow as tf
import numpy as np
import scipy
import ffprod

from tqdm import tqdm

key_dict_total = [
 '制作',
 '高速制作',
 '集中制作',
 '模范制作',
 '注视制作',
 '坯料制作',
 '加工',
 '仓促',
 '集中加工',
 '中级加工',
 '注视加工',
 '坯料加工',
 '精密制作',
 '专心加工',
 '俭约加工',
 # '工匠的神速技巧',
 '坚信',
 '闲静',
 '元素之印记',
 '比尔格的祝福',
 '俭约',
 '长期俭约',
 '改革',
 '崇敬',
 '元素之美名',
 '阔步',
 '精修',
 '内静',
 '观察',
 '最终确认',
 # '掌握', 
 '秘诀'
 ]

key_dict_no_rate = [
 '制作',
 #'高速制作',
 '集中制作',
 '模范制作',
 '注视制作',
 '坯料制作',
 '加工',
 #'仓促',
 '集中加工',
 '中级加工',
 '注视加工',
 '坯料加工',
 '精密制作',
 '专心加工',
 '俭约加工',
 # '工匠的神速技巧',
 '坚信',
 '闲静',
 '元素之印记',
 '比尔格的祝福',
 '俭约','俭约','俭约','俭约',
 '长期俭约','长期俭约','长期俭约',
 '改革','改革','改革','改革',
 '崇敬','崇敬','崇敬','崇敬',
 '元素之美名',
 '阔步',
 '精修',
 '内静',
 '观察',
 '最终确认',
 # '掌握', 
 '秘诀'
 ]

key_dict = key_dict_no_rate

key_count = len(key_dict)
macro_length = 15
level = 80
double_mapping = 2
single_mapping = 2

mutation_rate = 0.3
total_population = 1000
select_population = 150

hq_rate = [0, 1, 0, 0] # no hq at all to ease the simulation

'''
agent: (W, b) W: [rs, vs+1] b; [vs+1]

'''

def sigmoid(x0=0, y0=0, dy=1, k0=0.25):
    dx = dy * (0.25 / k0)
    return lambda x: (1 / (1 + np.exp((x0-x)/dx))) * dy + y0

def get_loss(p, q, e, z):
    lp1 = sigmoid(p*0.75, 0, 1.5, 1.5/p)
    lp2 = sigmoid(p*1.75, 0, 1.5, -1.5/p)
    lp = lambda x: lp1(x)*lp1(x)*lp1(x)*lp2(x)*0.5
    lq_ = sigmoid(q*0.8, 0, 0.7, 5/q)
    lq = lambda x: 0.3 * x / q + lq_(x)
    le_ = sigmoid(e+7.5, 0, 1, -1/7.5)
    le = lambda x: le_(x) + min(0, -(x-e-7.5))
    lz = sigmoid(z*1.25, 0, 1, -2.2/z)
    return lambda P, Q, E, Z: lp(P) + lq(Q)*6*lp1(P) + le(E)+lz(Z)+4*min(min(lp(P), lq(Q)), min(le(E), lz(Z)))
                                                      
quick_repr = lambda a: [key_dict[x] for x in a[0]]
def represent(agent, d=key_dict, l=level):
    m1 = [d[x] for x in agent]
    m2 = [ffprod.get_opr(x, ffprod.oprs, level=level) for x in m1]
    return m2

def get_new_agent(c=key_count, l=macro_length):
    return [np.random.randint(c) for i in range(l)]

def reproduce_agent(a1, a2, kc=key_count, mr=mutation_rate):
    mr2 = mr * 0.7
    mask = np.random.choice(3, size=len(a1), p=[(1-mr2)/2, (1-mr2)/2, mr2])
    a = [a1, a2]
    ans = [a[mask[i]][i] if mask[i] < 2 else np.random.randint(kc) for i in range(len(a1))]
    mask2 = np.random.choice(2, size=len(a1), p=[1-mr2 / 2, mr2 / 2])
    for i in range(len(a1) - 1, -1, -1):
        if mask2[i] == 1:
            mask3 = np.random.randint(4)
            if mask3 == 0:
                del ans[i]
                ans.append(np.random.randint(kc))
            elif mask3 == 1:
                del ans[i]
                ans = [np.random.randint(kc)] + ans
            elif mask3 == 2:
                ans = ans[:i] + [np.random.randint(kc)] + ans[i:]
                del ans[-1]
            elif mask3 == 3:
                ans = ans[:i] + [np.random.randint(kc)] + ans[i:]
                del ans[0]
            else:
                raise Exception("WTF")
    return ans
    

def gen_population(getnew = lambda: get_new_agent(key_count, macro_length), count=total_population):
    return [getnew() for i in range(count)]

def eval_agent(agent, goal, hq_rate_dict=hq_rate):
    sim = [(1, ffprod.init_state())]
    for m in represent(agent):
        sim = ffprod.simulate_seq(m, sim, goal, max_count=3000, hq_rate_dict=hq_rate_dict)
    loss = get_loss(*goal)
    ans1 = 0
    ans2 = 0
    for res in sim:
        ans1 += res[0]
        ans2 += res[0] * loss(*(res[1][1: 5]))
    ans2 /= ans1
    return ans1, ans2

def eval_population(pop, goal, hq_rate=hq_rate):
    d = []
    for agent in pop:
        d.append((agent, eval_agent(agent, goal, hq_rate_dict=hq_rate)[1]))
    return d

def select_and_regen(pop_eval, tp=total_population, sp=select_population, mr=mutation_rate):
    pop_eval.sort(key=lambda x: x[1], reverse=True)
    pop_eval = pop_eval[:sp]
    ans = [x[0] for x in pop_eval]
    for _ in range(tp-sp):
        a1 = pop_eval[np.random.randint(sp)][0]
        a2 = pop_eval[np.random.randint(sp)][0]
        a3 = reproduce_agent(a1, a2, mr=mr)
        if isinstance(a3, float):
            print(a1, a2)
        ans.append(a3)
    return ans
    pass

if __name__ == '__main__':
    try:
        assert pop
    except:
        pop = gen_population()
    #pop = gen_population()
    goal = (1265.5251141552512, 5325.375939849624, 70, 507)
    for _ in tqdm(range(1000)):
        ev = eval_population(pop, goal)
        print(ev[0][1], np.mean([x[1] for x in ev[:100]]))
        pop = select_and_regen(ev)
        