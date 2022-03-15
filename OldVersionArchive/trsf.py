
import numpy as np
import pandas as pd
from prettytable import PrettyTable

import utils

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

oprs = read_data('data.csv')

def trsf_name(s):
    ans = ''
    for s_ in s.split('_'):
        ans += s_[0].upper() + s_[1:]
    return ans

template = '\n\
        public static Action {name} = new()\n\
        {{\n\
            NameKey = "{name}",\n\
            LeastLevel = {least_level},\n\
            DProcess = {d_process},\n\
            DQuality = {d_quality},\n\
            SuccessRate = {success_rate},\n\
            DForce = {d_force},\n\
            DEndurance = {d_endurance},\n\
            DInnerStatic = {d_inner_static},\n\
            Effects =\n\
{eff_str},\n\
            TimedEffects = new REI(new DEI()\n\
            {{\n\
{teff_str}\n\
            }}),\n\
        }};\n\
\n\
'

for item in oprs:
    sp = item['sp_eff']
    tm = item['timed_eff']
    spstr = ' |\n'.join(['                ActionEffect.' + trsf_name(x) for x in sp]) if sp else '                ActionEffect.None'
    tmstr = ',\n'.join([f'                [Effect.{trsf_name(x)}] = {tm[x]}' for x in tm if tm[x] > 0])
    d = dict(item)
    d['eff_str'] = spstr
    d['teff_str'] = tmstr
    print(template.format(**d))
