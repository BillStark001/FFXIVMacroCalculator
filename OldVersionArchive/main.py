# -*- coding: utf-8 -*-
"""
Created on Wed Sep 15 19:26:47 2021

@author: billstark001
"""

import genetic
import utils
import ffprod

import numpy as np
import argparse

parser = argparse.ArgumentParser(description='Calculate the macro of operation codes used by craftsmen for a specified recipe in Final Fantasy XIV.')
parser.add_argument('recipe_path', metavar='path', type=str,
                    help='the recipe path')

if __name__ == '__main__':
    args = parser.parse_args()
    rpath = args.recipe_path
    json = utils.jload(rpath)
    recipe = utils.parse_args(json)
    
    goal, opr_dict, hq_dict, args_ga, args_sys = recipe
    macro_length, player_level, max_iteration, output_iteration = args_sys
    total_population = args_ga[0]
    opr_count = len(opr_dict)
    
    # generate population
    pop = [genetic.get_new_agent(opr_count, macro_length) for i in range(total_population)]
    
    for it in range(max_iteration):
        try:
            ev = genetic.eval_population(pop, goal, opr_dict, player_level, hq_dict)
            print(f'Iteration #{it}: best={ev[0][1]}, mean_of_top={np.mean([x[1] for x in ev[:100]])}')
            pop = genetic.select_and_regen(ev, opr_count, args_ga)
            if it % output_iteration == 0:
                with open(f'./output_iter_{it}.txt', 'w') as f:
                    w = ffprod.gen_output_lines([genetic.quick_repr(ev[x], opr_dict) for x in range(5)], goal)
                    f.writelines([x + '\n' for x in w])
        except KeyboardInterrupt:
            for m in ffprod.gen_output_lines([genetic.quick_repr(ev[0], opr_dict)], goal): print(m)
            break
