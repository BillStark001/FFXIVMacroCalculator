namespace FfxivMacroCalculator.ProductionSystem
{
    public class SimulateResult
    {
        public bool Succeed = false;
        public List<(double, WorkState)> PossibleStates = new();

        public SimulateResult(bool succeed, List<(double, WorkState)> states)
        {
            Succeed = succeed;
            PossibleStates = states;
        }
        
    }

    public class RecipeGoal
    {
        public int GP;
        public int GQ;
        public int GE;
        public int GZ;
    }

    public static class Simulator
    {
        //  = (0, 0.65, 0.25, 0.1)
        public static SimulateResult simulate_state(
            this WorkState state,
            Skill opr,
            RecipeGoal goal, 
            double total_rate=1,
            (double, double, double, double) hq_rate_dict)
        {
            if (opr.LeastLevel <= 0)
            {
                return new(false, new() { (total_rate, state) });
            }
    
            var (N, P, Q, E, Z, I, H, L) = state
            var (GP, GQ, GE, GZ) = goal
            L = dict(L);

            // judge if operation is legal

            var flag = true;
            if (opr.Contains(SpecialEffect.OnlyFirst) && state.WorkTime > 0)
                flag = false;
            if (opr.Contains(SpecialEffect.HQOnly) && state.QualityBelow(QualityState.High))
                flag = false;

            if 'only_first' in opr['sp_eff'] and N > 0: flag = false
            if 'hq_only' in opr['sp_eff'] and H < 2: flag = false
            if 'element_mark_appended' in opr['sp_eff'] and 'element_mark_appended' in L: flag = false
            if 'not_in_frugal' in opr['sp_eff'] and 'endurance_50p' in L: flag = false
            if 'only_when_inner_static' in opr['sp_eff'] and I == 0: flag = false
            if 'add_inner_static' in opr['sp_eff'] and I > 0: flag = false
    
            // judge if the state is halting
            flag2 = true
            if E >= GE: flag2 = false
            if P >= GP: flag2 = false
            if Z + opr['d_force'] > GZ: flag2 = false
    
            flag = flag and flag2
    
            if not flag:
                return flag, [[total_rate, state]]
    
            // calculate p q e s
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
    
            // execute current operation
    
            P_ = P + opr['d_process'] * p_rate
            Q_ = Q + opr['d_quality'] * q_rate
            E = E + opr['d_endurance'] * e_rate
            Z = Z + df
            I_ = I + opr['d_inner_static'] if I > 0 or 'add_inner_static' in opr['sp_eff'] else 0
    
            // TODO BUG!!!
            if 'endurance_add5' in L and \
                not ('no_work_time' in opr['sp_eff']) and \
                not ('endurance_add5' in opr['timed_eff']):
                if E < GE: E -= 5
    
            if 'reset_inner_static' in opr['sp_eff']:
                I_ = 0
            if 'div_inner_static' in opr['sp_eff']:
                I_ = I * 2
                I = int(I / 2) + 1
        
    
        
            // if I > 11: I = 11
            if I_ > 11: I_ = 11
            if Z < 0: Z = 0
            if E < 0: E = 0
    
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
        
            // add effects
    
            for eff in opr['timed_eff']:
                oe = opr['timed_eff'][eff]
                if oe > 0:
                    L_[eff] = oe
        
            anss = [[(1 - s_rate) * total_rate, (N, P, Q, E, Z, I, H, L)], [s_rate * total_rate, (N, P_, Q_, E, Z, I_, H, L_)]]
    
            // divide H
    
            ansss = []
            for ans in anss:
                a, b = ans
                if a <= 0: continue
                n, p, q, e, z, i, h, l = b
            
                if h == 3: 
                    ansss.append([a, (n, p, q, e, z, i, 0, l)])
                // doubtful
                elif h != 1: 
                    ansss.append([a, (n, p, q, e, z, i, 1, l)])
                else: 
                    for h_, hr in zip([0, 1, 2, 3], hq_rate_dict):
                        if hr <= 0: continue
                        ansss.append([a * hr, (n, p, q, e, z, i, h_, l)])
    
            // update L by operation
            return flag, ansss
        } 

    }
}
