using System.ComponentModel.DataAnnotations;
namespace PainTrax.Web.Models
{
    public class IntakeForm
    {
        // 1.Initial Evaluation Intake sheet
        public string? visit_type { get; set; }
        public string? location_id { get; set; }
        public string? l_name { get; set; }
        public string? f_name { get; set; }
        public string? case_type { get; set; }
        public DateTime? dob { get; set; }
        public DateTime? doa { get; set; }
        public DateTime? dov { get; set; }
        public string? wcb { get; set; }
        public int? age { get; set; }
        public string? gender { get; set; }
        public string? handedness { get; set; }
        public string? occupation { get; set; }
        public string? still_working { get; set; }
        public string? degree_of_disability { get; set; }
        public DateTime? last_day_worked { get; set; }
        public DateTime? return_to_work { get; set; }
        public int? weight { get; set; }
        public int? height { get; set; }

        // 2.Description of the accident
        public string? description_of_the_accident { get; set; }

        //3.History given by patient
        public string? direct_impact_on_bodypart { get; set; }
        public string? if_yes_which_bodypart { get; set; }
        public string? loss_consciousness { get; set; }
        public string? if_yes_how_long { get; set; }
        public string? did_go_to_the_hospital { get; set; }
        public string? if_yes_which_hospital { get; set; }
        public string? go_to_the_sameday { get; set; }
        public string? if_no_how_many_days_later { get; set; }
        public string? how_did_go_to_the_hospital { get; set; }
        public string? did_they_did_any_test { get; set; }
        public string? what_tests { get; set; }
        public string? body_parts { get; set; }
        public string? prescribed_any_medication { get; set; }
        public string? what_medication { get; set; }
        public string? involved_accident_past { get; set; }
        public string? when_accident_past { get; set; }
        public string? bodyparts_injured_inaccident { get; set; }
        public string? completly_recovered { get; set; }
        public string? any_medical_conditions { get; set; }
        public string? medical_conditions_others { get; set; }
        public string? had_any_surgeries { get; set; }
        public string? had_any_surgeries_when { get; set; }
        public string? type_of_surgery { get; set; }
        public string? taking_medications_surgery { get; set; }
        public string? what_medications_surgery { get; set; }
        public string? allergies { get; set; }
        public string? what_allergies { get; set; }
        public string? family_history { get; set; }
        public string? smoke { get; set; }
        public string? txt_smoke { get; set; }
        public string? marijuana { get; set; }
        public string? txt_marijuana { get; set; }
        public string? alcohol { get; set; }
        public string? txt_alcohol { get; set; }
        public string? physical_therapy { get; set; }
        public string? txt_physical_therapy { get; set; }
        public string? chiropractic { get; set; }
        public string? txt_chiropractic { get; set; }
        public string? symptoms_of_accident { get; set; }

        //4.ADL ( Activities of daily living )
        //public string? personal_hygiene { get; set; }
        //public string? eating_independently { get; set; }
        //public string? Shop_for_food { get; set; }
        //public string? walking { get; set; }
        //public string? cooking { get; set; }
        //public string? sleeping { get; set; }
        public string? able_to_perform { get; set; }
        public string? not_able_to_perform { get; set; }
        public string? difficluty_to_perform { get; set; }

        // 5.How would you describe your pain?

        public string? txt_describe_neck { get; set; }
        public string? describe_neck { get; set; }
        public string? neck_pain_radiates { get; set; }
        public string? neck_pain_bodypart { get; set; }
        public string? neck_pain_numbness { get; set; }
        public string? neck_pain_bodypart_numbness { get; set; }
        public string? txt_describe_midback { get; set; }
        public string? describe_midback { get; set; }
        public string? txt_describe_lowback { get; set; }
        public string? describe_lowback { get; set; }
        public string? lowback_pain_radiates { get; set; }
        public string? lowback_pain_bodypart { get; set; }
        public string? lowback_pain_numbness { get; set; }
        public string? lowback_pain_bodypart_numbness { get; set; }
        public string? describe_leftshoulder { get; set; }
        public string? txt_describe_leftshoulder { get; set; }
        public string? describe_rightshoulder { get; set; }
        public string? txt_describe_rightshoulder { get; set; }
        public string? describe_leftknee { get; set; }
        public string? txt_describe_leftknee { get; set; }
        public string? describe_rightknee { get; set; }
        public string? txt_describe_rightknee { get; set; }
        public string? other_describe_part { get; set; }
        public string? other_describe_part_value { get; set; }
        public string? txt_other_describe_part { get; set; }
        public string? other_describe_part_1 { get; set; }
        public string? other_describe_part_1_value { get; set; }
        public string? txt_other_describe_part_1 { get; set; }

        //6.What relieves your pain?

        public string? relieves_pain { get; set; }
        public string? txt_relieves_pain { get; set; }

        //7.What increases your pain?
        public string? increase_neck_pain { get; set; }
        public string? increase_midback_pain { get; set; }
        public string? increase_lowback_pain { get; set; }
        public string? increase_leftshoulder_pain { get; set; }
        public string? increase_rightshoulder_pain { get; set; }
        public string? increase_leftknee_pain { get; set; }
        public string? increase_rightknee_pain { get; set; }

        //8.Range of Motion (For Reference - normal values in brackets)
        public string? gait { get; set; }
        public string? txt_gait { get; set; }
        public string? cervical_pain { get; set; }
        public string? cervical_pain_trigger_points { get; set; }
        public string? rom_ff_fortyfive { get; set; }
        public string? rom_ext_fortyfive { get; set; }
        public string? rom_lat_bend_fortyfive_L { get; set; }
        public string? rom_lat_bend_fortyfive_R { get; set; }
        public string? rom_rot_eighty_L { get; set; }
        public string? rom_rot_eighty_R { get; set; }
        public string? rom_with_pain { get; set; }
        public string? spurlings_positive { get; set; }
        public string? spurlings_bilaterally { get; set; }
        public string? txt_spurlings { get; set; }
        public string? tinels_positive { get; set; }
        public string? tinels_bilaterally { get; set; }
        public string? phalens_positive { get; set; }
        public string? phalens_bilaterally { get; set; }
        public string? facet_positive { get; set; }
        public string? facet_bilaterally { get; set; }

        


        ////9.Neuro. Exam
        public string? dtrs_two_excepts_biceps_L { get; set; }
        //public string? dtrs_two_excepts_biceps_R { get; set; }
        public string? triceps_L { get; set; }
        //public string? triceps_R { get; set; }
        public string? brachioradialis_L { get; set; }
        //public string? brachioradialis_R { get; set; }
        public string? sensory { get; set; }
        public string? upper_ext_normal { get; set; }
        public string? except { get; set; }
        public string? weakness { get; set; }
        public string? thoracic { get; set; }
        public string? thoracic_trigger_points { get; set; }
        public string? lumbar { get; set; }
        public string? lumbar_trigger_points { get; set; }
        public string? rom_ff_ninety { get; set; }
        public string? rom_ext_30 { get; set; }
        public string? rom_lat_bend_30_L { get; set; }
        public string? rom_lat_bend_30_R { get; set; }
        public string? neuro_rom_rot_eighty_L { get; set; }
        public string? neuro_rom_rot_eighty_R { get; set; }
        public string? neuro_rom_with_pain { get; set; }
        public string? slr_positive { get; set; }
        public string? slr_bilaterally { get; set; }
        public string? neuro_facet_positive { get; set; }
        public string? neuro_facet_bilaterally { get; set; }
        public string? dtrs_two_except_pattler { get; set; }
        public string? achilles { get; set; }
        public string? planter { get; set; }
        public string? sensory_1 { get; set; }
        public string? lower_ext_normal { get; set; }
        public string? except_1 { get; set; }
        public string? weakness_1 { get; set; }
        public string? ls_swelling { get; set; }
        public string? ls_echymosis { get; set; }
        public string? ls_deformity { get; set; }
        public string? ls_masses { get; set; }
        public string? ls_tenderness { get; set; }
        public string? ls_acjoint { get; set; }
        public string? ls_abd_oneeighty_L { get; set; }
        public string? ls_abd_oneeighty_R { get; set; }
        public string? ls_flex_oneeighty_L { get; set; }
        public string? ls_flex_oneeighty_R { get; set; }
        public string? ls_add_thirty_L { get; set; }
        public string? ls_add_thirty_R { get; set; }
        public string? ls_with_pain { get; set; }
        public string? ls_ext_sixty_L { get; set; }
        public string? ls_ext_sixty_R { get; set; }
        public string? ls_ext_rot_ninety_L { get; set; }
        public string? ls_ext_rot_ninety_R { get; set; }
        public string? ls_int_rot_seventy_L { get; set; }
        public string? ls_int_rot_seventy_R { get; set; }
        public string? ls_with_pain_1 { get; set; }
        public string? ls_hawkins { get; set; }
        public string? ls_o_brien_test { get; set; }
        public string? ls_impegement { get; set; }
        public string? ls_drop_arm { get; set; }
        public string? ls_cross_over { get; set; }
        public string? ls_empty_can { get; set; }
        public string? rs_swelling { get; set; }
        public string? rs_echymosis { get; set; }
        public string? rs_deformity { get; set; }
        public string? rs_masses { get; set; }
        public string? rs_tenderness { get; set; }
        public string? rs_acjoint { get; set; }
        public string? rs_abd_oneeighty_L { get; set; }
        public string? rs_abd_oneeighty_R { get; set; }
        public string? rs_flex_oneeighty_L { get; set; }
        public string? rs_flex_oneeighty_R { get; set; }
        public string? rs_add_thirty_l { get; set; }
        public string? rs_add_thirty_R { get; set; }
        public string? rs_with_pain { get; set; }
        public string? rs_ext_sixty_L { get; set; }
        public string? rs_ext_sixty_R { get; set; }
        public string? rs_ext_rot_ninety_L { get; set; }
        public string? rs_ext_rot_ninety_R { get; set; }
        public string? rs_int_rot_seventy_L { get; set; }
        public string? rs_int_rot_seventy_R { get; set; }
        public string? rs_with_pain_1 { get; set; }
        public string? rs_hawkins { get; set; }
        public string? rs_o_brien_test { get; set; }
        public string? rs_impegement { get; set; }
        public string? rs_drop_arm { get; set; }
        public string? rs_cross_over { get; set; }
        public string? rs_empty_can { get; set; }
        public string? lk_swelling { get; set; }
        public string? lk_echymosis { get; set; }
        public string? lk_deformity { get; set; }
        public string? lk_masses { get; set; }
        public string? lk_effusion { get; set; }
        public string? lk_crepitus { get; set; }
        public string? lk_tenderness { get; set; }
        public string? lk_metal_joint_line { get; set; }
        public string? lk_flex_oneforty_L { get; set; }
        public string? lk_flex_oneforty_R { get; set; }
        public string? lk_ext_zero_L { get; set; }
        public string? lk_ext_zero_R { get; set; }
        public string? lk_with_pain { get; set; }
        public string? lk_mcmurray { get; set; }
        public string? lk_anterior { get; set; }
        public string? lk_lachman { get; set; }
        public string? lk_posterior { get; set; }
        public string? lk_valgus { get; set; }
        public string? lk_varus { get; set; }
        public string? rk_swelling { get; set; }
        public string? rk_echymosis { get; set; }
        public string? rk_deformity { get; set; }
        public string? rk_masses { get; set; }
        public string? rk_effusion { get; set; }
        public string? rk_crepitus { get; set; }
        public string? rk_tenderness { get; set; }
        public string? rk_metal_joint_line { get; set; }
        public string? rk_flex_oneforty_L { get; set; }
        public string? rk_flex_oneforty_R { get; set; }
        public string? rk_ext_zero_L { get; set; }
        public string? rk_ext_zero_R { get; set; }
        public string? rk_with_pain { get; set; }
        public string? rk_mcmurray { get; set; }
        public string? rk_anterior { get; set; }
        public string? rk_lachman { get; set; }
        public string? rk_posterior { get; set; }
        public string? rk_valgus { get; set; }
        public string? rk_varus { get; set; }
        public string? neuro_others { get; set; }
        //10.DIAGNOSES: 
        public string? diagonosis { get; set; }

        //11.RECOMMENDATIONS: 
        public string? diag_test_refferal { get; set; }
        public string? recommend_providerseen { get; set; }
        public string? recommend_providerseen_other { get; set; }
        public string? refferal { get; set; }
        public string? refferal_other { get; set; }
        public string? request_schedule { get; set; }
        public string? request_schedule_other { get; set; }
        //12.DME Dispensed at today's office visit:
        public string? dme_dispenced { get; set; }

        //13.DME Meadowbrook Medical Supplies (other provider)
        public string? dme_meadowbrook { get; set; }
        //14.Procedure done today:
        public string? procedure_done_today { get; set; }

        //15.Followup:
        public string? followup { get; set; }






    }
}
