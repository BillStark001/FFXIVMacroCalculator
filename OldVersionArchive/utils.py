# -*- coding: utf-8 -*-
"""
Created on Wed Sep 15 17:24:49 2021

@author: billstark001
"""

import json

def jdump(obj, path, indent=2, ensure_ascii=False):
    with open(path, 'w', encoding='utf-8') as f:
        json.dump(obj, f, indent=indent, ensure_ascii=ensure_ascii)
        
def jload(path):
    with open(path, 'r', encoding='utf-8') as f:
        ans = json.load(f)
    return ans

opr_dict_raw = jload('opr_dict.json')
hq_dict_allow = [0, 0.65, 0.25, 0.1]
hq_dict_disallow = [0, 1, 0, 0]
default_args_ga = dict(P=1500, SR=0.15, PR=0.15, MR=0.3)
default_args_sys = (15, 80, 100000, 20)

def get_opr_dict(allow_prob_skills=False, 
                 allow_manipulation=False, 
                 allow_trained_eye=False):
    ans = opr_dict_raw['no_prob']
    if allow_prob_skills: ans += opr_dict_raw['prob']
    if allow_manipulation: ans += [opr_dict_raw['manipulation']]
    if allow_trained_eye: ans += [opr_dict_raw['trained_eye']]
    return ans

# recipe goal related

# craftsmanship, control
def calculate_base_effect(d_level, cm, ct, suggested_cm, suggested_ct) :
    # d_level: player_level - recipe_level
    d_level = min(max(-30, d_level), 20)
    
    # level difference factor
    if d_level < -20:
        ldf_cm = 0.8 + 0.02 * (d_level + 30)
        ldf_ct = 0.6 + 0.04 * (d_level + 30)
    elif d_level > 0:
        ldf_cm = [1, 1.05, 1.1, 1.15, 1.2, 1.25, 1.27, 1.29, 1.31, 1.33, 
                  1.35, 1.37, 1.39, 1.41, 1.43, 1.45, 1.46, 1.47, 1.48, 1.49, 1.5][d_level]
        ldf_ct = 1
    else:
        ldf_cm = ldf_ct = 1    
    
    dp = int((ldf_cm * int(0.21 * cm + 2) * int(10000 + cm)) / int(10000 + suggested_cm))
    dq = int((ldf_ct * int(0.35 * ct + 35) * int(10000 + ct)) / int(10000 + suggested_ct))
    
    return dp, dq

def convert_goal(dp, dq, fp, fq):
    p = 100 * fp / dp
    q = 100 * fq / dq
    return p, q

def parse_args(args):
    p = args['Player']
    r = args['Recipe']
    s = args['System']
    plv = p['LV'] if 'LV' in p else args_sys[1]
    f = r['Format']
    if f == 1:
        raise Exception('Not supported yet')
        te = plv >= 80 # todo
        d_level = 0 # todo support this
        dpdq = calculate_base_effect(d_level, p['CM'], p['CT'], r['SCM']. r['SCT'])
    else:
        te = r['TrainedEye']
        dpdq = (r['DP'], r['DQ'])
    pq = convert_goal(*dpdq, r['P'], r['Q'] - (r['IQ'] if 'IQ' in r else 0))
    goal_raw = pq + (r['E'], p['CP'])
    goal = list(goal_raw)
    buffs = p['Buffs'] if 'Buffs' in p else []
    for buff in buffs:
        # TODO CM, CT
        bcm, bct, bcp, bmcm, bmct, bmcp = buff
        ocm = p['CM']
        oct_ = p['CT']
        ocp = goal[3]
        goal[3] = int(min(ocp + bmcp, ocp * (1 + bcp)))
        
    
    opr_dict = get_opr_dict(
        s['AllowProbSkills'] if 'AllowProbSkills' in s else False, 
        p['Manipulation'], 
        te)
    hq_dict = hq_dict_allow if 'AllowHQ' in s and s['AllowHQ'] else hq_dict_disallow
    args_ga = s['Arguments'] if 'Arguments' in s else default_args_ga
    args_ga = (int(args_ga['P']), 
               int(args_ga['SR']*args_ga['P']), 
               int(args_ga['PR']*args_ga['P']), 
               float(args_ga['MR']))
    args_sys = (s['MacroLength'] if 'MacroLength' in s else default_args_sys[0], 
                plv, 
                s['MITC'] if 'MITC' in s else default_args_sys[2], 
                s['OITC'] if 'OITC' in s else default_args_sys[3] 
                )
    
    return goal, opr_dict, hq_dict, args_ga, args_sys
    
    
        
    
        
        
    