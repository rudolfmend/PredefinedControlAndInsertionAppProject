   M S B A M L   `   `   `  ����i  ePredefinedControlAndInsertionAppProject, Version=1.0.9225.39293, Culture=neutral, PublicKeyToken=nulla5clr-namespace:PredefinedControlAndInsertionAppProject'PredefinedControlAndInsertionAppProject  5   	   8    2PredefinedControlAndInsertionAppProject.MainWindow   5   	   W SPresentationCore, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35e aSystem.Windows.Controls.Ribbon, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089R NWindowsBase, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35F 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     5   	   R NSystem.Xaml, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   Glocal5clr-namespace:PredefinedControlAndInsertionAppProject     5   	     � Title$*  $Predefined Control And Insertion App��5   	   $	��880��66   $
��1200��6C    � WindowStartupLocation$ CenterScreen=�5	   	   .��5
      � $��10q�6    � RowDefinitions 5   
   �� 5      $
��Auto �6   �� 5      $
��Auto �6   �� 5      $��* �6   �� 5      $
��Auto �6   .+�5   
   �� $��0��6   $��0,0,0,10q�6"   .+�5      �� $*��$Predefined Control And Insertion App��6   $��22(�5      $
��Bold"�6&   $��0,0,0,5q�68   �� 5      $3��-Automate interactions with other applications��6   $��14(�5      $��0,0,0,10q�6&   �� 5      5      �� 5   
   $��1��6   $&�� Target Application Configuration��6    $��	0,10,0,10q�6J   .��5      � $��5q�6    5      �� 5      $
��Auto �6$   �� 5       $
��Auto �6$   �� 5!      $
��Auto �6$    � ColumnDefinitions 5#      �� 5$      $
��Auto �6'   �� 5%      $��* �6'   �� 5&      $
��Auto �6'   �� 5'      $
��Auto �6'   .+�5+      �� -   5        $��BtnDetectApp��5+   6   $��0��6   $��3��6&   $��Detect Apps...��6J   $��0,5,0,5q�6c   $
��10,5q�5,      6(   �� 5/      $��1��6   $��0��6%   $��Window Title:��65   $��0,5,5,5q�6M   $��Center=�50      �� 51      -   5        $��txtWindowTitle��51   K   $��1��6   $��1��6'   $��2��67   $��0,5,0,5q�52      $��5q�6+   $��Center=�67   �� 55      $��2��6   $��0��6*   $��3��6:    �� Orientation$ 
Horizontal=�6N   $��0,10,0,0q�56      .+�57      �� -   5         ��Name$ BtnStartCapture��57      $��Start Element Capture��66   $
��10,5q�6V   $��0,0,10,0q�58      6/   �� 59      -   5        $ BtnLaunchApp��59      $��Launch Target Application��63   $
��10,5q�6W   5:      5;      �� 5<      $��0��6   $��0��6%   $��Running Application:��65   $��0,5,5,5q�6T   $��Center=�6e   �� 5=      -   5        $��CmbRunningApps��5=   8   $��0��6   $��1��6(   $��0,5,5,5q�6N   $��5q�6_   $��Center=�6k   �� 5>      -   5        $��BtnRefreshApps��5>   6   $��0��6   $��2��6&   $��Refresh��6L   $��0,5,5,5q�6^   $
��10,5q�6o   6~   5?      5@      �� 5C   
   $��2��6   $��	0,10,0,10q�6"   .@�5D      �� $��UI Elements��6   .��5E      � $��5q�6    5F      �� 5G      $��* �6(   �� 5H      $
��Auto �6(   .+�5K      &   System.Windows.Controls.DataGrid  -   5        $��
dgElements��5K   ,   $��0��6      AutoGenerateColumns$ False��6>   $��	0,10,0,10q�6Z      CanUserAddRows$
 True��5L         CanUserDeleteRows$
 True��65   	   Columns	 5M      0  *System.Windows.Controls.DataGridTextColumn  5N      ,  &System.Windows.Controls.DataGridColumn
   Header$

 Name��61      Width5  /System.Windows.Controls.DataGridLengthConverter$	 150 6X   1  +System.Windows.Controls.DataGridBoundColumn   Binding 6?   �� *Name+6X     5O      $

 Type��61   $	 100 6_    6?   �� *ElementType+6_     5P      $
 Automation ID��61   $	 150 6i    6H   �� *AutomationId+6i     5Q      $
 Value��61   $ * 6Z    6@   �� *Value+6Z   4  .System.Windows.Controls.DataGridCheckBoxColumn  5R      $
 	Is Target��65   $ 80 6e    6H   �� *
IsTarget+6e   5T      �� 5V      $��1��6!   $ 
Horizontal=�6.   .+�5W      �� -   5        $��BtnAddElement��5W   !   $��Add Element��66   $
��10,5q�6L   $��0,0,5,0q�6[   5X   !   �� 5Y      -	   5        $��BtnRemoveElement��5Y   !   $��Remove Element��69   $
��10,5q�6R   5Z   !   5[      5\      5]      �� 5^      $��Calculations��6   .��5_      � $��5q�6    5`      �� 5a      $
��Auto �6(   �� 5b      $��* �6(   .+�5e      �� $��0��6    $,��&Define calculations for target fields:��6-   $��	0,10,0,10q�6[     5g      -
   5        $��dgCalculations��5g   ,   $��1��6   $ False��6B   $
 True��5h      $
 True��65   	 5i        5j      $
 Target Field��61   $	 150 6g    6G   �� *TargetField+6g   4  .System.Windows.Controls.DataGridComboBoxColumn  5k      $
 	Operation��65   $	 120 5l   ^      SelectedItemBinding 64   �� *	Operation+6^     5m      $
 Value 1��61   $	 100 6]    6B   �� *Value1+6]     5n      $
 Value 2��61   $	 100 6]    6B   �� *Value2+6]     5o      $
 Formula��61   $ * 6^    6B   �� *	Formula+6^   5q      5r      5s      �� 5t      $��Automation Sequence��6   .��5u      � $��5q�6    5v      �� 5w      $
��Auto �6(   �� 5x      $��* �6(   �� 5y      $
��Auto �6(   .+�5|      �� $��0��6    $.��(Define the sequence of automation steps:��6-   $��	0,10,0,10q�6]   �� 5~      -   5        $��
LbSequence��5~   +   $��1��6   $��0,0,0,10q�6=    �� SelectionMode$ Extended=�6~   ��5      �� 5�      .R�5�   "   �  5�   &   �� 5�   *   $��30 �6;   �� 5�   *   $��80 �6;   �� 5�   *   $��* �6;   .+�5�   &   �� $��0��60   $��0,0,5,0q�6\   ��6@   �� *
StepNumber+6\   �� 5�   &   $��1��60   $��0,0,5,0q�6X   $��SemiBold"�6i   ��6@   �� *Action+6i   �� 5�   &   $��2��60   $��0,0,0,0q�6X   ��6@   �� *Target+6X   5�   #   5�      5�      �� 5�      $��2��6!   $ 
Horizontal=�6.   .+�5�      �� -   5        $ 
BtnAddStep��5�   !   $��Add Step��65   $
��10,5q�6H   $��0,0,5,0q�6W   5�      �� 5�      -   5        $ BtnEditStep��5�   !   $��	Edit Step��66   $
��10,5q�6J   $��0,0,5,0q�6Y   5�      �� 5�      -   5        $ BtnRemoveStep��5�   !   $��Remove Step��68   $
��10,5q�6N   $��0,0,5,0q�6]   5�      �� 5�      -   5        $ 	BtnMoveUp��5�   !   $��Move Up��64   $
��10,5q�6F   $��0,0,5,0q�6U   5�      �� 5�      -   5        $ BtnMoveDown��5�   !   $��	Move Down��66   $
��10,5q�6J   5�      �� 5�      -   5        $ 
BtnAddLoop��5�   !   $��Add Loop��65   $
��10,5q�6H   $��5,0,0,0q�5�   !   5�      5�      5�      5�      � 5�   
   $��3��6    5�      �� 5�      $��* �6#   �� 5�      $
��Auto �6#   .+�5�      �� $��0��6   $ 
Horizontal=�6)   .+�5�      �� -   5        $��	chkRepeat��5�      $��Repeat automation��6,   $��Center=�6H   $��0,0,10,0q�6c   �� 5�      $��Interval (seconds):��6   $��Center=�66   $��0,0,5,0q�6Q   �� 5�      -   5        $��txtInterval��5�      $��50��6-   $��Center=�68   $	��5,2q�6Z   5�      �� 5�      $��1��6   $ 
Horizontal=�6)   .+�5�      �� -   5        $��BtnSaveConfig��5�      $��Save Configuration��6.   $
��10,5q�6K   $��0,0,10,0q�6Z   5�      �� 5�      -   5        $��BtnLoadConfig��5�      $��Load Configuration��6.   $
��10,5q�6K   $��0,0,10,0q�6Z   5�      �� 5�      -   5        $��
BtnExecute��5�      $��Execute Automation��6+   $
��10,5q�6H   ���	#FF007ACC
�������$
��Bold"�5�   ,   6>   5�      5�      5�      5�      