# -*- coding: utf-8 -*-
"""
Created on Mon Sep 13 01:53:48 2021

@author: billstark001
"""

import numpy as np
import ffprod
import random

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
 #'集中制作',
 '模范制作',
 '注视制作',
 '坯料制作',
 '加工',
 #'仓促',
 #'集中加工',
 '中级加工',
 '注视加工',
 '坯料加工',
 '精密制作',
 # '专心加工',
 '俭约加工',
 # '工匠的神速技巧',
 '坚信',
 '闲静',
 '元素之印记',
 '比尔格的祝福',
 '俭约','俭约','俭约',
 '长期俭约','长期俭约', 
 '改革','改革','改革',
 '崇敬','崇敬','崇敬',
 '元素之美名',
 '阔步',
 '精修',
 '内静',
 '观察','观察','观察','观察',
 #'掌握', 
 #'秘诀', 
 '最终确认'
 ]

key_dict = key_dict_no_rate + ['掌握'] * 8

key_count = len(key_dict)
macro_length = 28
level = 80
double_mapping = 2
single_mapping = 2

mutation_rate = 0.4
total_population = 1500
select_population = int(total_population * 0.15)
preserve_population = int(total_population * 0.25)

hq_rate = [0, 1, 0, 0] # no hq at all to ease the simulation

'''
agent: (W, b) W: [rs, vs+1] b; [vs+1]

'''

def sigmoid(x0=0, y0=0, dy=1, k0=0.25):
    dx = dy * (0.25 / k0)
    return lambda x: (1 / (1 + np.exp((x0-x)/dx))) * dy + y0

def get_loss(p, q, e, z):
    lp1 = sigmoid(p*0.75, 0, 1.5, 1.5/p)
    lpup = lp1(p*1.05)**3
    lp = lambda x: min(lp1(x)**3 / lpup, 1) * 1.2
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
    mask2 = np.random.choice(2, size=len(a1), p=[1-mr2, mr2])
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
    mask3 = np.random.choice(2, size=len(a1) - 1, p=[1-mr2, mr2])
    for i in range(len(a1) - 1):
        if mask3[i] == 1:
            x = ans[i]
            ans[i] = ans[i+1]
            ans[i+1] = x
    return ans
    

def gen_population(getnew = lambda: get_new_agent(key_count, macro_length), count=total_population):
    return [getnew() for i in range(count)]

def eval_agent(agent, goal, hq_rate_dict=hq_rate, t_fr=0.9):
    sim = [(1, ffprod.init_state())]
    loss_fr = 0
    for i, m in enumerate(represent(agent)):
        sim, fr = ffprod.simulate_seq(m, sim, goal, max_count=3000, hq_rate_dict=hq_rate_dict)
        if fr > t_fr:
            loss_fr += 1
    loss = get_loss(*goal)
    ans1 = 0
    ans2 = 0
    for res in sim:
        ans1 += res[0]
        ans2 += res[0] * loss(*(res[1][1: 5]))
    ans2 /= ans1
    ans2 -= loss_fr * 0.01
    return ans1, ans2

def eval_population(pop, goal, hq_rate=hq_rate):
    d = []
    for agent in pop:
        d.append((agent, eval_agent(agent, goal, hq_rate_dict=hq_rate)[1]))
    d.sort(key=lambda x: x[1], reverse=True)
    return d

def select_and_regen(pop_eval, 
                     tp=total_population, 
                     sp=select_population, 
                     pp=preserve_population, 
                     mr=mutation_rate):
    pop_eval.sort(key=lambda x: x[1], reverse=True)
    pop_eval_s = pop_eval[:sp]
    pop_eval_p = pop_eval[sp:]
    random.shuffle(pop_eval[sp:])
    pop_eval_p = pop_eval_p[:pp]
    ans = [x[0] for x in pop_eval_s + pop_eval_p]
    psp = pop_eval_s + pop_eval_p
    for _ in range(tp-sp-pp):
        a1 = psp[np.random.randint(sp+pp)][0]
        a2 = psp[np.random.randint(sp+pp)][0]
        a3 = reproduce_agent(a1, a2, mr=mr)
        if isinstance(a3, float):
            print(a1, a2)
        ans.append(a3)
    return ans
    pass

def gshxd(x=0): 
    q = quick_repr(ev[x])
    ffprod.eval_on_average(q, goal)
    o = ffprod.output(q)
    for oo in o: 
        print()
        print(oo)
        print()

if __name__ == '__main__':
    np.set_printoptions(suppress=True)
    try:
        assert pop
    except:
        pop = gen_population()
    #goal = ffprod.get_goal_by_data(585, 631, 2214, 32860, 60, 415) # 伊修加德70
    #goal = ffprod.get_goal_by_data(442, 537, 5543, 28331, 70, 522) # 前言礼裙
    goal = ffprod.get_goal_by_data(447, 552, 7414, 46553, 70, 522) # 唯美装备
    for it in range(100000):
        try:
            ev = eval_population(pop, goal)
            print(f'Iteration #{it}: best={ev[0][1]}, mean_of_top={np.mean([x[1] for x in ev[:100]])}')
            pop = select_and_regen(ev)
        except KeyboardInterrupt:
            gshxd()
            break
        