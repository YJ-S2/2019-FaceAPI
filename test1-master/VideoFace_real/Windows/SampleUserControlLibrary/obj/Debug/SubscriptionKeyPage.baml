   M S B A M L   `   `   `  ����S  OSampleUserControlLibrary, Version=1.0.0.0, Culture=neutral, PublicKeyToken=nullC&clr-namespace:SampleUserControlLibrarySampleUserControlLibrary  5      2    ,SampleUserControlLibrary.SubscriptionKeyPage   5      \ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5      R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5      Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5      @d2http://schemas.microsoft.com/expression/blend/2008     5      8local&clr-namespace:SampleUserControlLibrary     5        N� Title$  SubscriptionKeyPage��5	       � 	Resources 5         
LinkButton �� 
TargetType �� Property �� Value �� 
TargetType �� Triggers P� Property P� Value��%F  &	        �� 5   
   " ��6#   .�5      ��  � �� 5      �� 5      " ��6&   .R�5      �� $��	Underline{�6$   �� 5      5      5      5      �� 5       � ��
 ��  �6+    5      P� 5       �@J�IsMouseOver . . �5      ��  � ��
 �  ��63   5      5      5      .��5      � .+�5   
   �� 	 �� Orientation$	 Vertical=�6   .+�5       �� $��
5, 0, 5, 0q�6   NLTo use the service, you need to ensure that you have right subscription key.6{   �� 5!      $��
5, 0, 5, 0q�6   ZXPlease note that each service (Face, Emotion, Speech, etc) has its own subscription key.6�   �� 5"      $��
5, 0, 5, 0q�6   pnIf you do not have key yet, please use the link to get a key first, then paste the key into the textbox below.6�   �� 5#      -   5        $��1��5#      $��3��6"   $
��Left=�6M   #��[  6h   $��
5, 0, 0, 0q�6�   $��Get Key��6�   � 5$      $��91��6   $��5,0,20,0q�6    
 � ColumnDefinitions
 5%      �� 5&      $	��70* �6'   �� 5'      $
��252* �6'   �� 5(      $
��253* �6'    � RowDefinitions 5*      �� 5+      $��1* �6$   �� 5,      $��1* �6$   �� 5-      $��1* �6$   .+�5/      �� $��Center=�6   $	��0,2q�63   $��26��6@   $��Subscription Key:��6L   �� 50      $��Stretch=�6   $��1��66   $��2q�6G   $��0,5,0,5q�6�   $��2��6�   ��6S   �� *SubscriptionKey+ �� Mode$ TwoWay=�6�   �� 51      $��Center=�6   $��1��63   $��	Endpoint:��6@   $	��0,2q�6T   $��26��6a   �� 52      $��Stretch=�6   $��1��66   $��1��6D   $��2q�6T   $��0,5,0,5q�6�   $��2��6�   ��6`   �� *SubscriptionEndpoint+$ TwoWay=�6�   �� 53      -   5        $��2��53   3   $��	78,7,73,6q�6@   $��
5, 0, 5, 0q�6S   $��Center=�6h   $��18��6�   $��Save Setting��6�   $��1��6�   �� 54      -   5        $��	75,7,76,6q�54   5   $��
5, 0, 5, 0q�6H   $��Center=�6]   $��18��6x   $��Delete Setting��6�   $��2��6�   $��2��6�   55      56      57      58      