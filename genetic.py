# -*- coding: utf-8 -*-
"""
Created on Mon Sep 13 01:53:48 2021

@author: billstark001
"""

import numpy as np
import ffprod
import utils
import random


# loss related

def sigmoid(x0=0, y0=0, dy=1, k0=0.25):
    dx = dy * (0.25 / k0)
    return lambda x: (1 / (1 + np.exp((x0-x)/dx))) * dy + y0

def get_loss(p, q, e, z):
    lp1 = sigmoid(p*0.75, 0, 1.5, 1.5/p)
    lp2 = lambda x: min(x/p, 1)**2.5
    lpup = lp1(p*1.05)**3
    lp = lambda x: min(lp1(x)**3 / lpup, 1) * 1.2
    lq_ = sigmoid(q*0.8, 0, 0.7, 5/q)
    lq = lambda x: 0.3 * x / q + lq_(x)
    le_ = sigmoid(e+7.5, 0, 1, -1/7.5)
    le = lambda x: le_(x) + min(0, -(x-e-7.5))
    lz = sigmoid(z*1.25, 0, 1, -2.2/z)
    return lambda P, Q, E, Z: lp(P) + lq(Q)*lp2(P)*6 + le(E)+lz(Z)+4*min(min(lp(P), lq(Q)), min(le(E), lz(Z)))
                                                     
quick_repr = lambda a, oprs: [oprs[x] for x in a[0]]

def represent(agent, oprs, level):
    m1 = [oprs[x] for x in agent]
    m2 = [ffprod.get_opr(x, ffprod.oprs, level=level) for x in m1]
    return m2

def get_new_agent(c, l):
    '''
    Parameters
    ----------
    c : int, the count of operation
    l : int, macro length

    Returns
    -------
    list, generated agent
    '''
    return [np.random.randint(c) for i in range(l)]

def reproduce_agent(a1, a2, kc, mr):
    '''
    Parameters
    ----------
    a1 : TYPE
        parent1.
    a2 : TYPE
        parent2.
    kc : TYPE
        count of operation.
    mr : TYPE
        mutation rate.
    '''
    mr2 = mr * 0.7
    a = [a1, a2]
    ans = []
    while len(ans) < len(a1):
        ans += a[np.random.choice(2)][len(ans): len(ans)+np.random.choice(6)]
    ans = ans[:len(a1)]
    #mask = np.random.choice(3, size=len(a1), p=[(1-mr2)/2, (1-mr2)/2, mr2])
    #ans = [a[mask[i]][i] if mask[i] < 2 else np.random.randint(kc) for i in range(len(a1))]
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
    

def eval_agent(agent, goal, oprs, level, hq_rate_dict, t_fr=0.9):
    sim = [(1, ffprod.init_state())]
    loss_fr = 0
    for i, m in enumerate(represent(agent, oprs, level)):
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

def eval_population(pop, goal, oprs, level, hq_rate):
    d = []
    for agent in pop:
        d.append((agent, eval_agent(agent, goal, oprs, level, hq_rate_dict=hq_rate)[1]))
    d.sort(key=lambda x: x[1], reverse=True)
    return d

def select_and_regen(pop_eval, opr_count, ga_args):
    tp, sp, pp, mr = ga_args
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
        a3 = reproduce_agent(a1, a2, opr_count, mr)
        if isinstance(a3, float):
            print(a1, a2)
        ans.append(a3)
    return ans
    pass

if __name__ == '__main__':
    np.set_printoptions(suppress=True)
    try:
        assert pop
    except:
        args_file = utils.jload('additional/gshxd.json')
        goal, opr_dict, hq_dict, args_ga, args_sys = utils.parse_args(args_file)
        macro_length, player_level, max_iteration, output_iteration = args_sys
        total_population = args_ga[0]
        opr_count = len(opr_dict)
        pop = [get_new_agent(opr_count, macro_length) for i in range(total_population)]
    for it in range(max_iteration):
        try:
            ev = eval_population(pop, goal, opr_dict, player_level, hq_dict)
            print(f'Iteration #{it}: best={ev[0][1]}, mean_of_top={np.mean([x[1] for x in ev[:100]])}')
            pop = select_and_regen(ev, opr_count, args_ga)
        except KeyboardInterrupt:
            for m in ffprod.gen_output_lines([quick_repr(ev[0], opr_dict)], goal): print(m)
            break
        