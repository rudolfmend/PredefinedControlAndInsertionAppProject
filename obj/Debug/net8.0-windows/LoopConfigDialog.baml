   M S B A M L   `   `   `  ����i  ePredefinedControlAndInsertionAppProject, Version=1.0.9225.39293, Culture=neutral, PublicKeyToken=nulla5clr-namespace:PredefinedControlAndInsertionAppProject'PredefinedControlAndInsertionAppProject  5   	   >    8PredefinedControlAndInsertionAppProject.LoopConfigDialog   5   	   W SPresentationCore, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35e aSystem.Windows.Controls.Ribbon, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089R NWindowsBase, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35F 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     5   	   R NSystem.Xaml, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   Glocal5clr-namespace:PredefinedControlAndInsertionAppProject     5   	     � Title$  Loop Configuration��5   	   $	��480��6$   $	��680��61    � WindowStartupLocation$ CenterOwner=�5	   	    � 
ResizeMode$ NoResize=�6-   .��5
      � $��15q�6    � RowDefinitions 5   
   �� 5      $
��Auto �6   �� 5      $
��Auto �6   �� 5      $
��Auto �6   �� 5      $
��Auto �6   �� 5      $
��Auto �6   �� 5      $��* �6   �� 5      $
��Auto �6   .+�5   
   �� $��0��6   $��Configure Loop Parameters��6!   $��16(�6B   $
��Bold"�6P   $��0,0,0,15q�6b   	� 5   
   -   5         	�Name$ rbFixedCount��5   #   $��1��6   $!��Fixed number of iterations:��69    	� 	GroupName$ LoopType��6_    d� 	IsChecked$
 TrueY�6t   $��0,0,0,10q�6�   �� 5   
   $��2��6    �� Orientation$ 
Horizontal=�6"   $��	20,0,0,15q�6;   .+�5      �� $��Number of iterations:��6   $��Center=�65   $��0,0,10,0q�6P   �� 5      -   5         ��Name$ txtIterationCount��5      $��80��61   $��10��6<   $��Center=�6F   5      	� 5    
   -   5        $ 
rbInfinite��5    #   $��3��6   $2��,Repeat indefinitely (until manually stopped)��67   $ LoopType��6n   $��0,0,0,15q�6�   	� 5#   
   -   5        $ rbExitCondition��5#   #   $��4��6   $$��Repeat until condition is met:��6<   $ LoopType��6e   $��0,0,0,10q�6z   � 5$   
   $��5��6   $��20,0,0,0q�6   	 � ColumnDefinitions	 5%      �� 5&      $
��Auto �6#   �� 5'      $��* �6#    5)      �� 5*      $
��Auto �6    �� 5+      $
��Auto �6    .+�5.      �� $��0��6   $��0��6%   $��Element:��65   $��Center=�6E   $��	0,0,10,10q�6`   �� 5/      -   5        
 ��Name$
 cmbExitElement��5/   4   $��0��6   $��1��6$   $��0,0,0,10q�6L    �� DisplayMemberPath$
 Name��6^   �� 51      $��1��6   $��0��6%   $��Value equals:��65   $��Center=�6J   $��0,0,10,0q�6e   �� 52      -   5        $ txtExitValue��52   3   $��1��6   $��1��6#   $��Center=�6I   53      �� 56   
   $��6��6   $ 
Horizontal=�6"   $��Right=�6;   $��0,15,0,0q�6W   .+�57      �� -   5        $��OK��57      $��80��6"   $��30��6-   $��0,0,10,0q�69    �� 	IsDefault$
 True��6_   �� 58      $��Cancel��6   $��80��6&   $��30��61    �� IsCancel$
 True��6=   59      5:      5;      