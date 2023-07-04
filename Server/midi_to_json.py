#!usr/bin/python
# coding: utf-8

from midi_extractor import *
from os import listdir
import argparse
import json

if __name__ == '__main__':

    parser = argparse.ArgumentParser()
    parser.add_argument('in_dirs', nargs='*', help='input directories that contain MIDI files')
    args = parser.parse_args()

    if len(args.in_dirs) >= 1:
        n_dirs = len(args.in_dirs)
        for j, in_dir in enumerate(args.in_dirs):
            print('Loading folder {} of {}: {}'.format(j + 1, n_dirs, in_dir))
            file_list = []
            mid_files = listdir(in_dir)

            def is_midi_file(filename):
                ext = filename[filename.rindex('.'): len(filename)]
                return ext == '.mid' or ext == '.MID' or ext == '.midi' or ext == '.MIDI'

            mid_files = list(filter(is_midi_file, mid_files))
            n = len(mid_files)
            mid_files.sort()
            assert(n > 0)
            print(mid_files)

            for i, in_path in enumerate(mid_files):
                fname = in_path.rstrip('.mid').rstrip('.MID').rstrip('.midi').rstrip('.MIDI')
                try:
                    dup = next(d for d in file_list if d == fname)
                    continue
                except StopIteration:
                    pass

                print('Processing file {} of {}: {}'.format(i + 1, n, in_path))
                if not (in_dir[-1] == '\\' or in_dir[-1] == '/'):
                    in_dir = in_dir + '/'

                try:
                    events, notes, ticks_per_beat = \
                        parse_events(in_dir.replace('\\', '/') + in_path)
                    
                    with open('../Client/Assets/Data/JSON/' + fname + '.json', 'w') as f:
                        json.dump(notes, f, indent=2)
                    
                except IOError as e:
                    print(e)
                    continue
    else:
        print('Usage:\n\tpython midi_to_json.py <in_dir_1> ... <in_dir_n>')